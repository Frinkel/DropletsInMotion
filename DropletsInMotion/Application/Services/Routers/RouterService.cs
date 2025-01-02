using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Security.Principal;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using static DropletsInMotion.Application.Services.Routers.Models.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debugger = DropletsInMotion.Infrastructure.Debugger;

namespace DropletsInMotion.Application.Services.Routers;
public class RouterService : IRouterService
{

    /*
     *  Actions:
     *  Move, SplitByVolume, SplitByRatio, Merge
     *
     *  Constraints:
     *  Single droplet routing
     *  
     */

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


        var reservedContaminationMap = _contaminationService.ReserveContaminations(commands, agents, _contaminationService.CloneContaminationMap(contaminationMap));

        foreach (var commandOrder in permutations)
        {
            Debugger.Permutations++;

            // Clear commitedStates for each new permutation attempt
            commitedStates.Clear();
            sFinal = null;

            bool foundSolution = true;

            foreach (var command in commandOrder)
            {

                // Create initial state and search for a solution
                State s0 = new State(command.GetInputDroplets().First(), agents, reservedContaminationMap, new List<IDropletCommand>() { command }, commitedStates, _contaminationService, _platformRepository, _templateRepository, Seed);
                Frontier f = new Frontier();
                sFinal = astarRouter.Search(s0, f);

                if (sFinal == null)
                {
                    foundSolution = false;
                    break;
                }


                // Combine states if a partial solution is found for this command
                commitedStates = CombineStates(commitedStates, sFinal);
            }

            // If a solution was found for this permutation, break out of the loop
            if (foundSolution && sFinal != null)
            {
                //chosenPermutation = commandOrder;
                break;
            }
        }

        //Console.WriteLine("Solution found with the following permutation:");
        //foreach (var c in chosenPermutation)
        //{
        //    Console.WriteLine(c);
        //}

        // If no solution was found across all permutations, throw an error
        if (sFinal == null)
        {
            throw new InvalidOperationException("No solution found in any permutation of the commands!");
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


        sFinal = FindFirstGoalState(sFinal, commands);

        // TODO: Alex can we segregate this?
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

                    //List<BoardAction> translatedActions = _templateService.ApplyTemplate(routeAction, parentAgents[dropletName], currentTime);

                    ITemplate? growTemplate = _templateRepository?.GrowTemplates?.Find(t => t.Direction == routeAction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
                    if (growTemplate != null)
                    {
                        finalActions.AddRange(growTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, currentTime, 1));
                    }

                    //boardActions.AddRange(growTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, time, 1));

                    //finalActions.AddRange(translatedActions);

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
        //remove reservations
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
                Console.WriteLine($"Agent {agentKvp.Key} did NOT exist in droplets!");
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

        // TODO: Debug?
        //_contaminationService.PrintContaminationMap(contaminationMap);
        //Console.WriteLine();
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
            throw new Exception($"Could not find {inputDroplets.First()} in the routable agents.");

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

        //Console.WriteLine($"TYPE IS: {command.GetType()}");
        //Console.WriteLine("Agent " + inputDroplets.First());
        //Console.WriteLine("Score so far: " + score);

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



    //public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    //{
    //    if (length == 1) return list.Select(t => new T[] { t });

    //    return GetPermutations(list, length - 1)
    //        .SelectMany(t => list.Where(e => !t.Contains(e)),
    //            (t1, t2) => t1.Concat(new T[] { t2 }));
    //}

    private List<State> CombineStates(List<State> commitedStates, State newState)
    {
        List<State> chosenStates = new List<State>();
        State currentState = newState;
        while (currentState.Parent != null)
        {
            chosenStates.Add(currentState);
            currentState = currentState.Parent;
        }
        chosenStates = chosenStates.OrderBy(s => s.G).ToList();

        // Get the last committed state
        State lastCommittedState = commitedStates.LastOrDefault();

        foreach (var state in chosenStates)
        {
            int index = commitedStates.FindIndex(s => s.G == state.G);
            if (index >= 0)
            {
                var commitedState = commitedStates[index];

                CombineState(commitedState, state);
            }
            else
            {
                // Before adding the new state, ensure it has the correct positions for other agents
                if (lastCommittedState != null)
                {
                    state.ContaminationMap = CombineContaminationMaps(lastCommittedState.ContaminationMap, state.ContaminationMap);
                    state.ContaminationMap = CombineContaminationMapAndContaminationChanges(state.ContaminationMap, state.ContaminationChanges);


                    foreach (var kvp in lastCommittedState.Agents)
                    {
                        string agentKey = kvp.Key;
                        if (agentKey != state.RoutableAgent)
                        {
                            state.Agents[agentKey] = kvp.Value;
                        }
                    }


                }
                commitedStates.Add(state);

                lastCommittedState = state;
            }
        }


        int maxGInChosenStates = chosenStates.Count > 0 ? chosenStates[^1].G : -1; 


        for (int i = 0; i < commitedStates.Count; i++)
        {
            var state = commitedStates[i];
            if (state.G > maxGInChosenStates)
            {
                // Update the state to reflect the last state in chosenStates
                var lastChosenState = chosenStates.LastOrDefault();
                if (lastChosenState != null)
                {
                    // Update the RoutableAgent in this state
                    state.Agents[lastChosenState.RoutableAgent] = (Agent)lastChosenState.Agents[lastChosenState.RoutableAgent].Clone();

                    // Update the ContaminationMap
                    state.ContaminationMap = CombineContaminationMaps(state.ContaminationMap, lastChosenState.ContaminationMap);

                    // Update the JointAction
                    if (lastChosenState.JointAction != null)
                    {
                        if (state.JointAction == null)
                        {
                            state.JointAction = new Dictionary<string, RouteAction>();
                        }
                        state.JointAction[lastChosenState.RoutableAgent] = lastChosenState.JointAction[lastChosenState.RoutableAgent];
                    }
                }
            }
        }

        return commitedStates;
    }

    private void CombineState(State oldState, State newState)
    {
        //oldState.ContaminationMap = CombineContaminationMaps(oldState.ContaminationMap, newState.ContaminationMap);
        oldState.ContaminationMap = CombineContaminationMapAndContaminationChanges(oldState.ContaminationMap, newState.ContaminationChanges);

        oldState.Agents[newState.RoutableAgent] = newState.Agents[newState.RoutableAgent];

        foreach (var action in newState.JointAction)
        {
            oldState.JointAction[action.Key] = action.Value;
        }
    }

    private List<int>[,] CombineContaminationMapAndContaminationChanges(List<int>[,] map1, Dictionary<(int x, int y), List<int>> contaminationChanges)
    {
        //int width = map1.GetLength(0);
        //int height = map1.GetLength(1);
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

    public void ApplyContaminationChangesToMap(
        Dictionary<(int x, int y), List<int>> contaminationChanges,
        List<int>[,] contaminationMap)
    {
        foreach (var kvp in contaminationChanges)
        {
            var (x, y) = kvp.Key;
            var changesList = kvp.Value;

            //// Ensure coordinates are within bounds
            //if (x < 0 || x >= contaminationMap.GetLength(0) ||
            //    y < 0 || y >= contaminationMap.GetLength(1))
            //{
            //    continue; // Skip out-of-bounds entries
            //}

            var mapList = contaminationMap[x, y];

            //if (mapList == null)
            //{
            //    // If the map list is null, create a new list with the changes
            //    contaminationMap[x, y] = new List<int>(changesList);
            //}
            //else
            //{
                // Add each substanceId from changesList to mapList if not already present
                foreach (var substanceId in changesList)
                {
                    if (!mapList.Contains(substanceId))
                    {
                        mapList.Add(substanceId);
                    }
                }
            //}
        }
    }

    //public void ApplyAllContaminationChangesToMap(State state, List<int>[,] contaminationMap)
    //{
    //    // Create a stack to hold states
    //    var stateStack = new Stack<State>();

    //    // Traverse up the parent chain and collect states
    //    while (state != null)
    //    {
    //        stateStack.Push(state);
    //        state = state.Parent;
    //    }

    //    // Apply contamination changes from the earliest state to the latest
    //    while (stateStack.Count > 0)
    //    {
    //        var currentState = stateStack.Pop();
    //        ApplyContaminationChangesToMap(currentState.ContaminationChanges, contaminationMap);
    //    }
    //}


    private List<int>[,] CombineContaminationMaps(List<int>[,] map1, List<int>[,] map2)
    {
        int width = map1.GetLength(0);
        int height = map1.GetLength(1);
        List<int>[,] resultMap = new List<int>[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create a new list for the result cell
                resultMap[i, j] = new List<int>(map1[i, j]);

                // Add values from map2, avoiding duplicates
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





    // USED ONLY FOR TEST
    //public void UpdateAgentSubstanceId(string agent, byte substanceId)
    //{
    //    Agents[agent].SubstanceId = substanceId;
    //    ContaminationMap = _contaminationService.ApplyContaminationMerge(Agents[agent], ContaminationMap);

    //}
    //// USED ONLY FOR TEST
    //public byte GetAgentSubstanceId(string agent)
    //{
    //    return Agents[agent].SubstanceId;
    //}
    //public byte[,] GetContaminationMap()
    //{
    //    return ContaminationMap;
    //}
    //public Dictionary<string, Agent> GetAgents()
    //{
    //    return Agents;
    //}
    //// USED ONLY FOR TEST
    //public void UpdateContaminationMap(int x, int y, byte value)
    //{
    //    ContaminationMap[x, y] = value;
    //}




}

