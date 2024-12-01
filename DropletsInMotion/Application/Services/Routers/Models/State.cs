using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using static DropletsInMotion.Application.Services.Routers.Models.Types;
using Debugger = DropletsInMotion.Infrastructure.Debugger;

namespace DropletsInMotion.Application.Services.Routers.Models;

public class State
{
    private int? Seed = null;
    public List<int>[,] ContaminationMap { get; set; }
    public Dictionary<string, Agent> Agents { get; set; }
    public int G { get; private set; }
    public State? Parent { get; set; }
    public Dictionary<string, RouteAction>? JointAction { get; set; }
    public RouteAction Action { get; private set; }
    private int H { get; set; }
    public string RoutableAgent { get; set; }
    public List<IDropletCommand> Commands { get; set; }

    public Dictionary<(int x, int y), List<int>> ContaminationChanges { get; set; }

    public List<State> CommitedStates { get; set; }

    private int? _cachedHash = null;

    private readonly IContaminationService _contaminationService;
    private readonly IPlatformRepository _platformRepository;
    private readonly ITemplateRepository _templateRepository;

    // Initial state
    public State(string routableAgent, Dictionary<string, Agent> agents, List<int>[,] contaminationMap, List<IDropletCommand> commands, List<State> commitedStates, IContaminationService contaminationService, IPlatformRepository platformRepository, ITemplateRepository templateRepository, int? seed = null)
    {
        Seed = seed;
        _contaminationService = contaminationService;
        _platformRepository = platformRepository;
        _templateRepository = templateRepository;
        RoutableAgent = routableAgent;
        Agents = new Dictionary<string, Agent>();
        foreach (var kvp in agents)
        {
            Agents[kvp.Key] = (Agent)kvp.Value.Clone();
        }
        ContaminationMap = _contaminationService.CloneContaminationMap(contaminationMap);
        Commands = commands;
        CommitedStates = commitedStates;
        JointAction = null;
        ContaminationChanges = new Dictionary<(int x, int y), List<int>>();
        Parent = null;
        G = 0;
        H = CalculateHeuristic();

    }

    public State(State parent, Types.RouteAction action)
    {
        Seed = parent.Seed;

        Parent = parent;
        RoutableAgent = parent.RoutableAgent;
        Commands = Parent.Commands;
        CommitedStates = Parent.CommitedStates;
        Action = action;
        JointAction = new Dictionary<string, RouteAction>();
        JointAction[RoutableAgent] = action;
        _contaminationService = Parent._contaminationService;

        ContaminationChanges = new Dictionary<(int x, int y), List<int>>();

        foreach (var kvp in Parent.ContaminationChanges)
        {
            List<int> newList = new List<int>(kvp.Value.Count);

            kvp.Value.ForEach((item) =>
            {
                newList.Add(item);
            });

            ContaminationChanges[kvp.Key] = newList;
        }


        ContaminationMap = Parent.ContaminationMap;

        _platformRepository = Parent._platformRepository;
        _templateRepository = Parent._templateRepository;

        G = Parent.G + 1;

        // Clone parent agents
        Agents = new Dictionary<string, Agent>();
        foreach (var kvp in Parent.Agents)
        {
            Agents[kvp.Key] = (Agent)kvp.Value.Clone();
        }

        //var watch = System.Diagnostics.Stopwatch.StartNew();
        //foreach (var actionKvp in jointAction)
        //{
        // EXECUTE THE ACTION
        Agent agent = Agents[RoutableAgent];
        agent.Execute(action);
        _contaminationService.ApplyContamination(agent, this);
        //}

        H = CalculateHeuristic();
        
        //watch.Stop();
        //var elapsedMs = watch.Elapsed.Microseconds;
        //Debugger.ElapsedTime.Add(elapsedMs);
    }


    public List<int> GetContamination(int x, int y)
    {
        List<int> contamination = new List<int>();
        contamination.AddRange(ContaminationMap[x, y]);

        if (ContaminationChanges.TryGetValue((x, y), out var values))
        {
            contamination.AddRange(values);
            return contamination;
        }
        else
        {
            return contamination;
        }
    }

    public void SetContamination(int x, int y, List<int> values)
    {
        ContaminationChanges[(x, y)] = values;
    }

    public List<BoardAction> ExtractActions(double time)
    {
        if (JointAction == null)
        {
            return new List<BoardAction>();
        }

        List<State> chosenStates = new List<State>();
        State currentState = this;
        while (currentState.Parent != null)
        {
            chosenStates.Add(currentState);
            currentState = currentState.Parent;
        }

        chosenStates = chosenStates.OrderBy(s => s.G).ToList();

        List<BoardAction> finalActions = new List<BoardAction>();

        Dictionary<string, double> currentTimes = new Dictionary<string, double>();
        Dictionary<string, bool> canRavel = new Dictionary<string, bool>();
        Dictionary<string, bool> canUnravel = new Dictionary<string, bool>();
        Dictionary<string, bool> hasRaveled = new Dictionary<string, bool>();
        Dictionary<string, double> endUnravel = new Dictionary<string, double>();


        foreach (var actionKvp in chosenStates[0].JointAction)
        {
            currentTimes[actionKvp.Key] = time;
        }


        double currentTime = time;

        State firstState = chosenStates.First();
        foreach (var agent in Agents)
        {
           string dropletName = agent.Key;
           endUnravel[dropletName] = 0;
           canRavel[dropletName] = false;
           hasRaveled[dropletName] = false;
            Agent currentAgent = firstState.Parent.Agents[dropletName];
            if (currentAgent.SnakeBody.Count == 1)
            {
                canUnravel[dropletName] = true;
            }
            else
            {
                canUnravel[dropletName] = false;
            }

        }

        State lastState = chosenStates.Last();
        foreach (var command in Commands)
        {
            var dropletName = command.GetInputDroplets().First();

            if (IsGoalState(command, lastState.Agents[dropletName]))
            {
                canRavel[dropletName] = true;
            }
        }


        var ravelActions = new List<BoardAction>();
        var unravelActions = new List<BoardAction>();
        var shrinkActions = new List<BoardAction>();

        foreach (State state in chosenStates)
        {
            foreach (var actionKvp in state.JointAction)
            {
                
                if (actionKvp.Value == Types.RouteAction.NoOp)
                {
                    continue;
                }
                string dropletName = actionKvp.Key;
                //string routeAction = actionKvp.Value.Name;
                var agents = state.Parent.Agents;
                Agent agent = agents[dropletName];
                Agent nextAgent = state.Agents[dropletName];
                
                var moveActions = ApplySnake(agent, nextAgent, actionKvp.Value, currentTime);
                
                finalActions.AddRange(moveActions);
                var tempTime = moveActions.Last().Time;

                if (canRavel[dropletName])
                {
                    var ravelAction = Ravel(lastState.Agents[dropletName], nextAgent, tempTime);
                    if (ravelAction != null)
                    {
                        ravelActions.AddRange(ravelAction);
                        canRavel[dropletName] = false;
                        hasRaveled[actionKvp.Key] = true;
                        //We need to wait with the shrink until the unrawel is complete.
                        var shrinkTime = endUnravel[dropletName] > tempTime ? endUnravel[dropletName] : tempTime;
                        shrinkActions.AddRange(ShrinkSnake(nextAgent, shrinkTime));
                    }
                }

                if (canUnravel[dropletName])
                {
                    var unravelAction = Unravel(firstState.Parent.Agents[dropletName], agent, nextAgent, hasRaveled[actionKvp.Key], currentTime);
                    if (unravelAction != null)
                    {
                        endUnravel[dropletName] = unravelAction.Count > 0 ? unravelAction.Last().Time : endUnravel[dropletName];
                        unravelActions.AddRange(unravelAction);
                        canUnravel[dropletName] = false;
                    }
                }

                
            }
            finalActions = finalActions.OrderBy(b => b.Time).ToList();

            //var totalTime = finalActions.Last().Time - currentTime;
            //currentTime = currentTime + (totalTime / scaleFactor);
            currentTime = finalActions.Last().Time;
        }

        shrinkActions = shrinkActions.OrderBy(b => b.Time).ToList();
        ravelActions = ravelActions.OrderBy(b => b.Time).ToList();

        finalActions = finalActions.OrderBy(b => b.Time).ToList();
        currentTime = finalActions.Last().Time;


        foreach (var command in Commands)
        {
            var dropletName = command.GetInputDroplets().First();
            Agent agent = lastState.Agents[dropletName];


            if (IsGoalState(command, agent))
            {
                while (agent.SnakeBody.Count > 1)
                {
                    agent.RemoveFromSnake();
                }
            }
        }




        finalActions.AddRange(unravelActions);
        finalActions.AddRange(shrinkActions);
        finalActions.AddRange(ravelActions);

        ravelActions = ravelActions.OrderBy(b => b.Time).ToList();
        finalActions = finalActions.OrderBy(b => b.Time).ToList();

        BoardActionUtils.FilterBoardActions(ravelActions, finalActions);

        return finalActions;
    }

    private List<BoardAction> ShrinkSnake(Agent agent, double time)
    {
        var boardActions = new List<BoardAction>();

        var snakeBody = agent.SnakeBody;

        while (snakeBody.Count > 1)
        {
            var last = snakeBody.Last();
            agent.RemoveFromSnake();
            var nextToLast = snakeBody.Last();
            string direction = FindDirection(nextToLast, last);

            ITemplate? shrinkTemplate = _templateRepository?.ShrinkTemplates?.Find(t => t.Direction == direction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
            boardActions.AddRange(shrinkTemplate.Apply(_platformRepository.Board[last.x][last.y].Id, time, 1));
            time = boardActions.Last().Time;
        }

        return boardActions;
    }

    private List<BoardAction> ApplySnake(Agent agent, Agent nextAgent, Types.RouteAction action,  double time)
    {
        var boardActions = new List<BoardAction>();

        ITemplate? growTemplate = _templateRepository?.GrowTemplates?.Find(t => t.Direction == action.Name && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
        if (growTemplate == null)
        {
            throw new Exception($"No grow template found for agent {agent.DropletName}");
        }

        boardActions.AddRange(growTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, time, 1));


        if (agent.SnakeBody.Count == nextAgent.SnakeBody.Count)
        {
            var tailPosition = agent.SnakeBody.Last();
            var tailPositionNext = nextAgent.SnakeBody.Last();
            string direction = FindDirection(tailPositionNext, tailPosition);

            ITemplate? shrinkTemplate = _templateRepository?.ShrinkTemplates?.Find(t => t.Direction == direction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
            if (shrinkTemplate != null)
            {
                boardActions.AddRange(shrinkTemplate.Apply(_platformRepository.Board[tailPosition.x][tailPosition.y].Id, time, 1));
            }
        }

        return boardActions;
    }

    private string FindDirection((int x, int y) tail1, (int x, int y) tail2)
    {
        var xDiff = tail1.x - tail2.x;
        var yDiff = tail1.y - tail2.y;

        string direction = (xDiff, yDiff) switch
        {
            ( > 0, _) => "moveRight",
            ( < 0, _) => "moveLeft",
            (_, > 0) => "moveDown",
            (_, < 0) => "moveUp",
            _ => ""
        };
        return direction;
    }


    private List<BoardAction> Unravel(Agent agentInitial, Agent agent, Agent nextAgent, bool mustUnravel, double time)
    {
        (int x, int y) newPosition = (nextAgent.PositionX, nextAgent.PositionY);

        if (!agentInitial.GetAllAgentPositions().Contains(newPosition) || mustUnravel)
        {
            ITemplate? unravelTemplate = _templateRepository?.UnravelTemplates?.Find(t =>
                t.FinalPositions.First().Value == (agent.PositionX - (agentInitial.PositionX - t.InitialPositions.First().Value.x), agent.PositionY - (agentInitial.PositionY - t.InitialPositions.First().Value.y))
                && t.MinSize <= agentInitial.Volume && agentInitial.Volume < t.MaxSize) ?? null;

            if (unravelTemplate == null)
            {
                return new List<BoardAction>();
            }

            return unravelTemplate.Apply(_platformRepository.Board[agentInitial.PositionX - unravelTemplate.InitialPositions.First().Value.x][agentInitial.PositionY - unravelTemplate.InitialPositions.First().Value.y].Id, time, 1);
        }

        return null;
    }

    private List<BoardAction> Ravel(Agent finalAgent, Agent nextAgent, double time)
    {
        (int x, int y) newPosition = (nextAgent.PositionX, nextAgent.PositionY);

        if (finalAgent.GetAllAgentPositions().Contains(newPosition))
        {

            ITemplate? ravelTemplate = _templateRepository?.RavelTemplates?.Find(t => 
                //Some magic to translate into the relative postion in order to compare
                t.InitialPositions.First().Value == (nextAgent.PositionX - (finalAgent.PositionX - t.FinalPositions.First().Value.x) , nextAgent.PositionY - (finalAgent.PositionY - t.FinalPositions.First().Value.y))
                && t.MinSize <= finalAgent.Volume && finalAgent.Volume < t.MaxSize) ?? null;

            if (ravelTemplate == null)
            {
                return new List<BoardAction>();
            }


            // Board[][] the wierd thing inside it to calculate the ofset of the template
            return ravelTemplate.Apply(_platformRepository.Board[finalAgent.PositionX - ravelTemplate.FinalPositions.First().Value.x][finalAgent.PositionY - ravelTemplate.FinalPositions.First().Value.y].Id, time, 1);
        }

        return null;
    }
    
    public List<State> GetExpandedStates()
    {

        List<State> expandedStates = new List<State>();

        List<Types.RouteAction> applicableActions = new List<Types.RouteAction>();

        Agent agent = Agents[RoutableAgent];
        List<Types.RouteAction> possibleActions = Types.RouteAction.PossiblActions;


        Dictionary<string, Agent> commitedAgentPositions = GetCommitedAgentPositions();
        var commitedContaminations =
            CommitedStates.Count > 0 ? CommitedStates.Last().ContaminationMap : ContaminationMap;

        foreach (Types.RouteAction action in possibleActions)
        {
            if (agent.IsMoveApplicable(action, commitedAgentPositions, commitedContaminations, this))
            {
                applicableActions.Add(action);
            }
        }


        foreach (var action in applicableActions)
        {
            State newState = new State(this, action);
            expandedStates.Add(newState);

            Debugger.ExpandedStates += 1;
        
        }

        //Random random;

        //random = Seed != null ? new Random((int)Seed) : new Random();

        //expandedStates = expandedStates.OrderBy(x => random.Next()).ToList();

        return expandedStates;
    }

    private Dictionary<string, Agent> GetCommitedAgentPositions()
    {
        if(CommitedStates.Count == 0)
        {
            return Agents;
        }
        return CommitedStates.Count > G ? CommitedStates[G].Agents : CommitedStates.Last().Agents;
    }


    private bool IsConflicting(Dictionary<string, Types.RouteAction> jointAction)
    {
        Dictionary<string, Tuple<int, int>> agentDestinations = new Dictionary<string, Tuple<int, int>>();
        foreach (var action in jointAction)
        {
            var agent = Agents[action.Key];
            agentDestinations.Add(action.Key, new Tuple<int, int>(agent.PositionX + action.Value.DropletXDelta, agent.PositionY + action.Value.DropletYDelta));
        }

        foreach (var action in jointAction)
        {
            if (action.Value.Type == Types.ActionType.NoOp)
            {
                continue;
            }

            foreach (var otherAction in jointAction)
            {
                if (action.Key == otherAction.Key || otherAction.Value.Type == Types.ActionType.NoOp)
                {
                    continue;
                }

                if (Math.Abs(agentDestinations[action.Key].Item1 - agentDestinations[otherAction.Key].Item1) <= 1 &&
                    Math.Abs(agentDestinations[action.Key].Item2 - agentDestinations[otherAction.Key].Item2) <= 1)
                {
                    return true;
                }
            }


        }

        return false;
    }

    static List<Dictionary<string, Types.RouteAction>> GetActionPermutations(Dictionary<string, List<Types.RouteAction>> agentActions)
    {
        var agents = agentActions.Keys.ToList();

        List<Dictionary<string, Types.RouteAction>> result = new List<Dictionary<string, Types.RouteAction>>();
        GeneratePermutations(agentActions, new Dictionary<string, Types.RouteAction>(), agents, 0, result);
        return result;
    }

    static void GeneratePermutations(
        Dictionary<string, List<Types.RouteAction>> agentActions,
        Dictionary<string, Types.RouteAction> current,
        List<string> agents,
        int depth,
        List<Dictionary<string, Types.RouteAction>> result)
    {
        if (depth == agents.Count)
        {
            result.Add(new Dictionary<string, Types.RouteAction>(current));
            return;
        }
        string agent = agents[depth];

        foreach (var action in agentActions[agent])
        {
            current[agent] = action;

            GeneratePermutations(agentActions, current, agents, depth + 1, result);
        }
    }


    public int GetFScore()
    {
        return H + G;
    }

    private int CalculateHeuristic()
    {
        int h = 0;
        foreach (IDropletCommand command in Commands)
        {
            if (command is Move moveCommand)
            {
                Agent agent = Agents[moveCommand.GetInputDroplets().First()];

                int manhattanDistance = Math.Abs(moveCommand.PositionX - agent.PositionX) +
                                        Math.Abs(moveCommand.PositionY - agent.PositionY);

                // Penalize states where the path to the goal is blocked
                if (IsPathBlocked(agent.PositionX, agent.PositionY, moveCommand.PositionX, moveCommand.PositionY, agent))
                {
                    manhattanDistance += 2;
                }

                // Penalize the act of standing still
                if (manhattanDistance != 0 && JointAction != null && JointAction[agent.DropletName].Type == Types.ActionType.NoOp)
                {
                    manhattanDistance += 1;
                }

                h += manhattanDistance;

            }
            else
            {
                throw new InvalidOperationException("Trying to calculate heuristic for unknown dropletCommand!");
            }
        }
        return h;
    }

    private bool IsPathBlocked(int startX, int startY, int endX, int endY, Agent agent, int maxDepth = 15)
    {
        int dx = Math.Abs(endX - startX);
        int dy = -Math.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1;
        int sy = startY < endY ? 1 : -1;
        int err = dx + dy, e2;
        int steps = 0;

        while (true)
        {
            if (_contaminationService.IsConflicting(ContaminationMap, startX, startY, agent.SubstanceId))
                return true;

            if (startX == endX && startY == endY) break;
            if (steps++ > maxDepth) break;

            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                startX += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                startY += sy;
            }
        }
        return false;
    }


    public bool IsGoalState()
    {
        foreach (var command in Commands)
        {
            if (!IsGoalState(command))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsGoalState(List<IDropletCommand> commands)
    {
        foreach (var command in commands)
        {
            if (!IsGoalState(command))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsOneGoalState()
    {
        foreach (var command in Commands)
        {
            if (IsGoalState(command))
            {
                return true;
            }
        }
        return false;

    }

    public bool IsPossibleEndState()
    {
        bool isOneGoalState = IsOneGoalState();

        if (!isOneGoalState) return isOneGoalState;
        

        bool allGoalsReached = IsGoalState();
        if (allGoalsReached) return allGoalsReached;
        
        //check that we dont terminate while we are in the process of unraveling a snake.
        foreach (var agent in Agents)
        {
            if (agent.Value.SnakeBody.Count < agent.Value.GetMaximumSnakeLength())
            {
                return false;
            }
        }
        return true;

    }


    public bool IsGoalState(IDropletCommand dropletCommand)
    {
        switch (dropletCommand)
        {
            case Move moveCommand:
                var agent = Agents[moveCommand.GetInputDroplets().First()];
                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
            default:
                throw new InvalidOperationException("Trying to determine goalstate for unknown dropletCommand!");
                break;
        }

    }

    public static bool IsGoalState(IDropletCommand dropletCommand, Agent agent)
    {
        switch (dropletCommand)
        {
            case Move moveCommand:
                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
            default:
                throw new InvalidOperationException($"Trying to determine goalstate for unknown dropletCommand! {dropletCommand}");
                break;
        }

    }

    public bool IsGoalStateReachable()
    {
        foreach (var command in Commands)
        {
            switch (command)
            {
                case Move moveCommand:
                    var agent = Agents[moveCommand.GetInputDroplets().First()];
                    //return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
                    if (_contaminationService.IsConflicting(ContaminationMap, moveCommand.PositionX, moveCommand.PositionY, agent.SubstanceId))
                    {
                        throw new InvalidOperationException(
                            $"Impossible for droplet {agent.DropletName} to reach the position in command {moveCommand}");
                    }
                    if (agent.GetAgentSize() + moveCommand.PositionX > ContaminationMap.GetLength(0) ||
                        agent.GetAgentSize() + moveCommand.PositionY > ContaminationMap.GetLength(1))
                    {
                        throw new InvalidOperationException(
                            $"Impossible for droplet {agent.DropletName} to reach the position in command {moveCommand} due to agent size.");
                    }


                    break;

                default:
                    break;
            }
        }

        return true;
    }


    public override int GetHashCode()
    {
        if (_cachedHash != null)
        {
            return _cachedHash.Value;
        }

        int hash = 17;

        //foreach (var value in ContaminationMap)
        //{
        //    hash = hash * 31 + value;
        //}
        //hash = hash * 31 + ContaminationMapHash;

        foreach (var agent in Agents.Values)
        {
            hash = hash * 31 + agent.PositionX.GetHashCode();
            hash = hash * 31 + agent.PositionY.GetHashCode();
        }

        _cachedHash = hash;
        return hash;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not State otherState)
            return false;

        if (!AreAgentsEqual(Agents, otherState.Agents))
            return false;

        //if (!AreContaminationMapsEqual(ContaminationMap, otherState.ContaminationMap))
        //    return false;

        return true;
    }

    //private bool AreContaminationMapsEqual(byte[,] map1, byte[,] map2)
    //{
    //    if (map1.GetLength(0) != map2.GetLength(0) || map1.GetLength(1) != map2.GetLength(1))
    //        return false;

    //    for (int i = 0; i < map1.GetLength(0); i++)
    //    {
    //        for (int j = 0; j < map1.GetLength(1); j++)
    //        {
    //            if (map1[i, j] != map2[i, j])
    //                return false;
    //        }
    //    }

    //    return true;
    //}

    private bool AreAgentsEqual(Dictionary<string, Agent> agents1, Dictionary<string, Agent> agents2)
    {
        if (agents1.Count != agents2.Count)
            return false;

        foreach (var kvp in agents1)
        {
            if (!agents2.TryGetValue(kvp.Key, out var agent2))
                return false;

            var agent1 = kvp.Value;
            if (agent1.PositionX != agent2.PositionX || agent1.PositionY != agent2.PositionY)
                return false;
        }

        return true;
    }
}


