using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Repositories;
using Debugger = DropletsInMotion.Infrastructure.Debugger;

namespace DropletsInMotion.Application.Services.Routers;
public class RouterService : IRouterService
{
    public int? Seed = null;
    private Electrode[][] Board { get; set; }
    
    private readonly IContaminationService _contaminationService;
    private readonly ITemplateService _templateService;
    private readonly IPlatformRepository _platformRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IContaminationRepository _contaminationRepository;


    public RouterService(IContaminationService contaminationService, ITemplateService templateService, IPlatformRepository platformRepository, ITemplateRepository templateRepository, IContaminationRepository contaminationRepository)
    {
        _templateService = templateService;
        _contaminationService = contaminationService;
        _platformRepository = platformRepository;
        _templateRepository = templateRepository;
        _contaminationRepository = contaminationRepository;
    }

    public void Initialize(Electrode[][] board, int? seed = null)
    {
        Seed = seed;
        Board = board;
        _templateService.Initialize(Board);
    }

    public List<BoardAction> Route(Dictionary<string, Agent> agents, List<IDropletCommand> commands, List<int>[,] contaminationMap, double time, double? boundTime = null)
    {
        AstarRouter astarRouter = new AstarRouter();

        List<State> commitedStates = new List<State>();
        State? sFinal = null;

        // Generate all permutations of the commands list
        var permutations = GetScoredPermutations(commands, commands.Count, command => ScoreCommand(command, agents));
        var chosenPermutation = permutations.First();

        var reservedContaminationMap = _contaminationService.ReserveContaminations(commands, agents, _contaminationService.CloneContaminationMap(contaminationMap));

        foreach (var commandOrder in permutations)
        {
            Debugger.Permutations++;

            commitedStates.Clear();
            sFinal = null;
            chosenPermutation = commandOrder;
            bool foundSolution = true;

            foreach (var command in commandOrder)
            {

                State s0 = new State(command.GetInputDroplets().First(), agents, reservedContaminationMap, new List<IDropletCommand>() { command }, commitedStates, _contaminationService, _platformRepository, _templateRepository, Seed);
                Frontier f = new Frontier();
                sFinal = astarRouter.Search(s0, f);

                if (sFinal == null)
                {
                    foundSolution = false;
                    break;
                }

                commitedStates = CombineStates(commitedStates, sFinal);
            }

            if (foundSolution && sFinal != null)
            {
                break;
            }
        }


        // If no solution was found across all permutations, throw an error
        if (sFinal == null)
        {
            throw new InvalidOperationException($"No solution found in any permutation of the commands! {Debugger.Permutations}");
        }

        for (int i = 1; i < commitedStates.Count; i++)
        {
            commitedStates[i].Parent = commitedStates[i - 1];
        }

        if (commitedStates.Count == 0)
        {
            return new List<BoardAction>();
        }

        sFinal = commitedStates.Last();

        //Console.WriteLine(chosenPermutation);
        sFinal = FindFirstGoalState(sFinal, commands);

        if (boundTime != null)
        {
            List<State> chosenStates = new List<State>();
            State currentState = sFinal;
            while (currentState.Parent != null)
            {
                chosenStates.Add(currentState);
                currentState = currentState.Parent;
            }

            chosenStates = chosenStates.OrderBy(s => s.G).ToList();

            List<BoardAction> finalActions = new List<BoardAction>();
            double currentTime = time;

            foreach (State state in chosenStates)
            {
                foreach (var actionKvp in state.JointAction)
                {
                    if (actionKvp.Value == Types.RouteAction.NoOp)
                    {
                        continue;
                    }

                    string dropletName = actionKvp.Key;
                    string routeAction = actionKvp.Value.Name;
                    var parentAgents = state.Parent.Agents;
                    var agent = parentAgents[dropletName];


                    ITemplate? growTemplate = _templateRepository?.GrowTemplates?.Find(t => t.Direction == routeAction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
                    if (growTemplate != null)
                    {
                        finalActions.AddRange(growTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, currentTime, 1));
                    }
                }

                finalActions = finalActions.OrderBy(b => b.Time).ToList();
                currentTime = finalActions.Last().Time;
                if (currentTime >= boundTime)
                {
                    sFinal = state;
                    break;
                }
            }
        }

        _contaminationService.CopyContaminationMap(sFinal.ContaminationMap, contaminationMap);

        // Remove contamination reservations
        _contaminationService.RemoveContaminations(commands, agents, contaminationMap);
        foreach (var (agentName, agent) in sFinal.Agents)
        {
            if (agent.SnakeBody.Count == 1)
            {
                _contaminationService.ApplyContaminationWithSize(agent, contaminationMap);
            }
            else
            {
                _contaminationService.ApplyContamination(agent, contaminationMap);
            }
        }


        foreach (var agentKvp in sFinal.Agents)
        {
            if (agents.ContainsKey(agentKvp.Key))
            {
                var agent = agentKvp.Value;
                agents[agentKvp.Key].PositionX = agent.PositionX;
                agents[agentKvp.Key].PositionY = agent.PositionY;
                agents[agentKvp.Key].SnakeBody = agent.SnakeBody;
            }
            else
            {
                throw new DropletNotFoundException($"Agent {agentKvp.Key} did NOT exist in droplets!");
            }
        }

        List<State> chosenStates2 = new List<State>();
        State currentState2 = sFinal;
        while (currentState2.Parent != null)
        {
            chosenStates2.Add(currentState2);
            currentState2 = currentState2.Parent;
        }
        chosenStates2 = chosenStates2.OrderBy(s => s.G).ToList();

        foreach (var command in commands)
        {
            string dropletName = command.GetInputDroplets().First();
            Agent agent = sFinal.Agents[dropletName];

            if (State.IsGoalState(command, agent))
            {
                _contaminationService.ApplyContaminationWithSize(agent, contaminationMap);
            }
        }

        return sFinal.ExtractActions(time);
    }

    private double ScoreCommand(IDropletCommand command, Dictionary<string, Agent> agents)
    {
        int substanceCount = _contaminationRepository.SubstanceTable.Count;
        int contaminationTableCount = _contaminationRepository.ContaminationTable.Count;
        int fullContaminatingCount = substanceCount - contaminationTableCount;

        var inputDroplets = command.GetInputDroplets();
        var success = agents.TryGetValue(inputDroplets.First(), out var agent);

        if (!success)
            throw new DropletNotFoundException($"Could not find {inputDroplets.First()} in the routable agents.");

        double score = 0;

        var substanceId = agent.SubstanceId;

        // Check if the agent's substance is in the contamination table
        var isInContaminationTable = _contaminationRepository.SubstanceTable[substanceId].Item2;
        if (isInContaminationTable)
        {

            for (int colIndex = 0; colIndex < contaminationTableCount; colIndex++)
            {
                var relation = _contaminationRepository.ContaminationTable[colIndex][substanceId];

                if (relation)
                {
                    score += 1;
                }
            }

            score += fullContaminatingCount;
        }
        else
        {
            score += substanceCount - 1;
        }

        return score;
    }

    public static IEnumerable<IEnumerable<IDropletCommand>> GetScoredPermutations(IEnumerable<IDropletCommand> moves, int length, Func<IDropletCommand, double> heuristic)
    {
        var scoredMoves = moves.OrderBy(heuristic);

        if (length == 1)
            return scoredMoves.Select(t => new[] { t });

        return GetScoredPermutations(scoredMoves, length - 1, heuristic)
            .SelectMany(t => scoredMoves.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new[] { t2 }));
    }


    private List<State> CombineStates(List<State> commitedStates, State newState)
    {
        List<State> chosenStates = new List<State>();
        State currentState = newState;
        while (currentState != null)
        {
            chosenStates.Add(currentState);
            currentState = currentState.Parent;
        }
        chosenStates = chosenStates.OrderBy(s => s.G).ToList();

        State? lastCommittedState = commitedStates.LastOrDefault();

        foreach (var state in chosenStates)
        {
            int index = commitedStates.FindIndex(s => s.G == state.G);
            if (index >= 0)
            {
                CombineState(commitedStates[index], state);

                lastCommittedState = commitedStates[index];
            }
            else
            {
                if (lastCommittedState != null)
                {
                    var cloneMap = _contaminationService.CloneContaminationMap(
                        lastCommittedState.ContaminationMap
                    );

                    cloneMap = CombineContaminationMaps(cloneMap, state.ContaminationMap);
                    cloneMap = CombineContaminationMapAndContaminationChanges(
                        cloneMap,
                        state.ContaminationChanges
                    );

                    state.ContaminationMap = cloneMap;

                    foreach (var kvp in lastCommittedState.Agents)
                    {
                        if (kvp.Key != state.RoutableAgent)
                        {
                            state.Agents[kvp.Key] = (Agent)kvp.Value.Clone();
                        }
                    }
                }
                commitedStates.Add(state);
                lastCommittedState = state;
            }
        }

        int maxG = chosenStates.Any() ? chosenStates[^1].G : -1;
        for (int i = 0; i < commitedStates.Count; i++)
        {
            var cs = commitedStates[i];
            if (cs.G > maxG && lastCommittedState != null)
            {
                var cloneMap = _contaminationService.CloneContaminationMap(
                    lastCommittedState.ContaminationMap
                );
                cs.ContaminationMap = cloneMap;
            }
        }

        return commitedStates;
    }

    private void CombineState(State oldState, State newState)
    {
        var mergedMap = _contaminationService.CloneContaminationMap(oldState.ContaminationMap);

        mergedMap = CombineContaminationMaps(mergedMap, newState.ContaminationMap);
        mergedMap = CombineContaminationMapAndContaminationChanges(
            mergedMap,
            newState.ContaminationChanges
        );

        oldState.ContaminationMap = mergedMap;

        oldState.Agents[newState.RoutableAgent] = newState.Agents[newState.RoutableAgent];

        if (newState.JointAction != null)
        {
            foreach (var kvp in newState.JointAction)
            {
                oldState.JointAction[kvp.Key] = kvp.Value;
            }
        }
    }

    private List<int>[,] CombineContaminationMapAndContaminationChanges(List<int>[,] map1, Dictionary<(int x, int y), List<int>> contaminationChanges)
    {
        List<int>[,] resultMap = map1;

        foreach (var kvp in contaminationChanges)
        {
            var (x, y) = kvp.Key;
            var changesList = kvp.Value;

            foreach (var substanceId in changesList)
            {
                if (!resultMap[x,y].Contains(substanceId))
                {
                    resultMap[x, y].Add(substanceId);
                }
            }
        }

        return resultMap;
    }

    private List<int>[,] CombineContaminationMaps(List<int>[,] map1, List<int>[,] map2)
    {
        int width = map1.GetLength(0);
        int height = map1.GetLength(1);
        List<int>[,] resultMap = new List<int>[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                resultMap[i, j] = new List<int>(map1[i, j]);

                foreach (var value in map2[i, j])
                {
                    if (!resultMap[i, j].Contains(value))
                    {
                        resultMap[i, j].Add(value);
                    }
                }
            }
        }

        return resultMap;
    }

    private State FindFirstGoalState(State state, List<IDropletCommand> commands)
    {
        List<State> chosenStates = new List<State>();
        State currentState = state;
        while (currentState.Parent != null)
        {
            currentState.Commands = commands;
            chosenStates.Add(currentState);
            currentState = currentState.Parent;
        }
        chosenStates = chosenStates.OrderBy(s => s.G).ToList();

        foreach (var returnState in chosenStates)
        {
            if (returnState.IsPossibleEndState())
            {
                return returnState;
            }
        }
        return state;
    }
}

