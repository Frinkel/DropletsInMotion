﻿//using System.ComponentModel;
//using System.Data;
//using System.Diagnostics;
//using System.Diagnostics.Contracts;
//using DropletsInMotion.Application.Execution.Models;
//using DropletsInMotion.Application.ExecutionEngine.Models;
//using DropletsInMotion.Application.Models;
//using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
//using DropletsInMotion.Infrastructure.Models.Platform;
//using DropletsInMotion.Infrastructure.Repositories;
//using static DropletsInMotion.Application.Services.Routers.Models.Types;
//using Debugger = DropletsInMotion.Infrastructure.Debugger;

//namespace DropletsInMotion.Application.Services.Routers.Models;

//public class StateOld
//{
//    private int? Seed = null;

//    public byte[,] ContaminationMap { get; private set; }
//    public Dictionary<string, Agent> Agents { get; private set; }
//    public int G { get; private set; }
//    public StateOld? Parent { get; private set; }
//    public Dictionary<string, RouteAction>? JointAction { get; private set; }

//    private int H { get; set; }
//    private List<string> RoutableAgents { get; set; }
//    public List<IDropletCommand> Commands { get; set; }

//    private int? _cachedHash = null;

//    private readonly IContaminationService _contaminationService;
//    private readonly IPlatformRepository _platformRepository;
//    private readonly ITemplateRepository _templateRepository;

//    // Initial state
//    public StateOld(List<string> routableAgents, Dictionary<string, Agent> agents, byte[,] contaminationMap, List<IDropletCommand> commands, IContaminationService contaminationService, IPlatformRepository platformRepository, ITemplateRepository templateRepository, int? seed = null)
//    {
//        Seed = seed;
//        _contaminationService = contaminationService;
//        _platformRepository = platformRepository;
//        _templateRepository = templateRepository;
//        RoutableAgents = routableAgents;
//        Agents = new Dictionary<string, Agent>();
//        foreach (var kvp in agents)
//        {
//            Agents[kvp.Key] = (Agent)kvp.Value.Clone();
//        }
//        ContaminationMap = (byte[,]) contaminationMap.Clone();
//        Commands = commands;
//        JointAction = null;

//        Parent = null;
//        G = 0;
//        H = CalculateHeuristic();

//    }

//    public StateOld(StateOld parent, Dictionary<string, Types.RouteAction> jointAction)
//    {
//        Seed = parent.Seed;

//        Parent = parent;
//        RoutableAgents = new List<string>(Parent.RoutableAgents);
//        ContaminationMap = (byte[,])Parent.ContaminationMap.Clone();
//        Commands = Parent.Commands;
//        JointAction = jointAction;
//        _contaminationService = Parent._contaminationService;
//        _platformRepository = Parent._platformRepository;
//        _templateRepository = Parent._templateRepository;

//        G = Parent.G + 1;

//        // Clone parent agents
//        Agents = new Dictionary<string, Agent>();
//        foreach (var kvp in Parent.Agents)
//        {
//            Agents[kvp.Key] = (Agent)kvp.Value.Clone();
//        }

//        //var watch = System.Diagnostics.Stopwatch.StartNew();
//        foreach (var actionKvp in jointAction)
//        {
//            Agent agent = Agents[actionKvp.Key];
//            agent.Execute(actionKvp.Value);
//            ContaminationMap = _contaminationService.ApplyContamination(agent, ContaminationMap);


//            // If a droplet is in its goal position we do not need to route it for child states
//            foreach (var command in Commands)
//            {
//                if (command is Move moveCommand)
//                {
//                    if (agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY)
//                    {
//                        RoutableAgents.Remove(agent.DropletName);
//                    }
//                }
//            }
//        }

//        H = CalculateHeuristic();
        
//        //watch.Stop();
//        //var elapsedMs = watch.Elapsed.Microseconds;
//        //Debugger.ElapsedTime.Add(elapsedMs);
//    }

//    public List<BoardAction> ExtractActions(double time)
//    {
//        if (JointAction == null)
//        {
//            return new List<BoardAction>();
//        }

//        List<StateOld> chosenStateOlds = new List<StateOld>();
//        StateOld currentStateOld = this;
//        while (currentStateOld.Parent != null)
//        {
//            chosenStateOlds.Add(currentStateOld);
//            currentStateOld = currentStateOld.Parent;
//        }

//        chosenStateOlds = chosenStateOlds.OrderBy(s => s.G).ToList();

//        List<BoardAction> finalActions = new List<BoardAction>();

//        Dictionary<string, double> currentTimes = new Dictionary<string, double>();
//        Dictionary<string, bool> canRavel = new Dictionary<string, bool>();
//        Dictionary<string, bool> canUnravel = new Dictionary<string, bool>();
//        Dictionary<string, bool> hasRaveled = new Dictionary<string, bool>();
//        Dictionary<string, double> endUnravel = new Dictionary<string, double>();


//        foreach (var actionKvp in chosenStateOlds[0].JointAction)
//        {
//            currentTimes[actionKvp.Key] = time;
//        }


//        double currentTime = time;

//        StateOld firstStateOld = chosenStateOlds.First();
//        foreach (var actionKvp in firstStateOld.JointAction)
//        {
//           string dropletName = actionKvp.Key;
//           endUnravel[dropletName] = 0;
//           canRavel[actionKvp.Key] = false;
//           hasRaveled[actionKvp.Key] = false;
//            Agent agent = firstStateOld.Parent.Agents[dropletName];
//                if (agent.SnakeBody.Count == 1)
//                {
//                    canUnravel[actionKvp.Key] = true;
//                }
//                else
//                {
//                    canUnravel[actionKvp.Key] = false;
//                }

//        }

//        StateOld lastStateOld = chosenStateOlds.Last();
//        foreach (var actionKvp in lastStateOld.JointAction)
//        {
//            IDropletCommand dropletCommand =
//                lastStateOld.Commands.Find(c => c.GetInputDroplets().First() == actionKvp.Key);

//            if (IsGoalStateOld(dropletCommand, lastStateOld.Agents[actionKvp.Key]))
//            {
//                canRavel[actionKvp.Key] = true;
//            }
//        }


//        var ravelActions = new List<BoardAction>();
//        var unravelActions = new List<BoardAction>();
//        var shrinkActions = new List<BoardAction>();

//        foreach (StateOld state in chosenStateOlds)
//        {
//            foreach (var actionKvp in state.JointAction)
//            {
                
//                if (actionKvp.Value == Types.RouteAction.NoOp)
//                {
//                    continue;
//                }
//                string dropletName = actionKvp.Key;
//                //string routeAction = actionKvp.Value.Name;
//                var agents = state.Parent.Agents;
//                Agent agent = agents[dropletName];
//                Agent nextAgent = state.Agents[dropletName];
                
//                var moveActions = ApplySnake(agent, nextAgent, actionKvp.Value, currentTime);
                
//                finalActions.AddRange(moveActions);
//                var tempTime = moveActions.Last().Time;

//                if (canRavel[dropletName])
//                {
//                    var ravelAction = Ravel(lastStateOld.Agents[dropletName], nextAgent, tempTime);
//                    if (ravelAction != null)
//                    {
//                        ravelActions.AddRange(ravelAction);
//                        canRavel[dropletName] = false;
//                        hasRaveled[actionKvp.Key] = true;
//                        //We need to wait with the shrink until the unrawel is complete.
//                        var shrinkTime = endUnravel[dropletName] > tempTime ? endUnravel[dropletName] : tempTime;
//                        shrinkActions.AddRange(ShrinkSnake(nextAgent, shrinkTime));
//                    }
//                }

//                if (canUnravel[dropletName])
//                {
//                    var unravelAction = Unravel(firstStateOld.Parent.Agents[dropletName], agent, nextAgent, hasRaveled[actionKvp.Key], currentTime);
//                    if (unravelAction != null)
//                    {
//                        endUnravel[dropletName] = unravelAction.Count > 0 ? unravelAction.Last().Time : endUnravel[dropletName];
//                        unravelActions.AddRange(unravelAction);
//                        canUnravel[dropletName] = false;
//                    }
//                }

                
//            }
//            finalActions = finalActions.OrderBy(b => b.Time).ToList();

//            //var totalTime = finalActions.Last().Time - currentTime;
//            //currentTime = currentTime + (totalTime / scaleFactor);
//            currentTime = finalActions.Last().Time;
//        }

//        shrinkActions = shrinkActions.OrderBy(b => b.Time).ToList();
//        ravelActions = ravelActions.OrderBy(b => b.Time).ToList();

//        finalActions = finalActions.OrderBy(b => b.Time).ToList();
//        currentTime = finalActions.Last().Time;


//        foreach (var actionKvp in lastStateOld.JointAction)
//        {
//            string dropletName = actionKvp.Key;
//            Agent agent = lastStateOld.Agents[dropletName];

//            IDropletCommand dropletCommand =
//                lastStateOld.Commands.Find(c => c.GetInputDroplets().First() == dropletName);

//            if (IsGoalStateOld(dropletCommand, agent))
//            {
//                while (agent.SnakeBody.Count > 1)
//                {
//                    agent.RemoveFromSnake();
//                }
//            }
//        }



//        finalActions.AddRange(unravelActions);
//        finalActions.AddRange(shrinkActions);
//        finalActions.AddRange(ravelActions);

//        ravelActions = ravelActions.OrderBy(b => b.Time).ToList();
//        finalActions = finalActions.OrderBy(b => b.Time).ToList();

//        BoardActionUtils.FilterBoardActions(ravelActions, finalActions);

//        return finalActions;
//    }

//    private List<BoardAction> ShrinkSnake(Agent agent, double time)
//    {
//        var boardActions = new List<BoardAction>();

//        var snakeBody = agent.SnakeBody;

//        while (snakeBody.Count > 1)
//        {
//            var last = snakeBody.Last();
//            agent.RemoveFromSnake();
//            var nextToLast = snakeBody.Last();
//            string direction = FindDirection(nextToLast, last);

//            ShrinkTemplate? shrinkTemplate = _templateRepository?.ShrinkTemplates?.Find(t => t.Direction == direction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
//            boardActions.AddRange(shrinkTemplate.Apply(_platformRepository.Board[last.x][last.y].Id, time, 1));
//            time = boardActions.Last().Time;
//        }

//        return boardActions;
//    }

//    private List<BoardAction> ApplySnake(Agent agent, Agent nextAgent, Types.RouteAction action,  double time)
//    {
//        var boardActions = new List<BoardAction>();

//        GrowTemplate? growTemplate = _templateRepository?.GrowTemplates?.Find(t => t.Direction == action.Name && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
//        if (growTemplate == null)
//        {
//            throw new Exception($"No grow template found for agent {agent.DropletName}");
//        }

//        boardActions.AddRange(growTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, time, 1));


//        if (agent.SnakeBody.Count == nextAgent.SnakeBody.Count)
//        {
//            var tailPosition = agent.SnakeBody.Last();
//            var tailPositionNext = nextAgent.SnakeBody.Last();
//            string direction = FindDirection(tailPositionNext, tailPosition);

//            ShrinkTemplate? shrinkTemplate = _templateRepository?.ShrinkTemplates?.Find(t => t.Direction == direction && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;
//            boardActions.AddRange(shrinkTemplate.Apply(_platformRepository.Board[tailPosition.x][tailPosition.y].Id, time, 1));
//        }

//        return boardActions;
//    }

//    private string FindDirection((int x, int y) tail1, (int x, int y) tail2)
//    {
//        var xDiff = tail1.x - tail2.x;
//        var yDiff = tail1.y - tail2.y;

//        string direction = (xDiff, yDiff) switch
//        {
//            ( > 0, _) => "moveRight",
//            ( < 0, _) => "moveLeft",
//            (_, > 0) => "moveDown",
//            (_, < 0) => "moveUp",
//            _ => throw new InvalidOperationException("No movement detected or invalid movement.")
//        };
//        return direction;
//    }


//    private List<BoardAction> Unravel(Agent agentInitial, Agent agent, Agent nextAgent, bool mustUnravel, double time)
//    {
//        (int x, int y) newPosition = (nextAgent.PositionX, nextAgent.PositionY);

//        if (!agentInitial.GetAllAgentPositions().Contains(newPosition) || mustUnravel)
//        {
//            UnravelTemplate? unravelTemplate = _templateRepository?.UnravelTemplates?.Find(t =>
//                t.FinalPositions.First().Value == (agent.PositionX - (agentInitial.PositionX - t.InitialPositions.First().Value.x), agent.PositionY - (agentInitial.PositionY - t.InitialPositions.First().Value.y))
//                && t.MinSize <= agentInitial.Volume && agentInitial.Volume < t.MaxSize) ?? null;

//            if (unravelTemplate == null)
//            {
//                return new List<BoardAction>();
//            }

//            Console.WriteLine("Template name: " + unravelTemplate.Name);

//            return unravelTemplate.Apply(_platformRepository.Board[agentInitial.PositionX - unravelTemplate.InitialPositions.First().Value.x][agentInitial.PositionY - unravelTemplate.InitialPositions.First().Value.y].Id, time, 1);
//        }

//        return null;
//    }

//    private List<BoardAction> Ravel(Agent finalAgent, Agent nextAgent, double time)
//    {
//        (int x, int y) newPosition = (nextAgent.PositionX, nextAgent.PositionY);

//        if (finalAgent.GetAllAgentPositions().Contains(newPosition))
//        {

//            RavelTemplate? ravelTemplate = _templateRepository?.RavelTemplates?.Find(t => 
//                //Some magic to translate into the relative postion in order to compare
//                t.InitialPositions.First().Value == (nextAgent.PositionX - (finalAgent.PositionX - t.FinalPositions.First().Value.x) , nextAgent.PositionY - (finalAgent.PositionY - t.FinalPositions.First().Value.y))
//                && t.MinSize <= finalAgent.Volume && finalAgent.Volume < t.MaxSize) ?? null;

//            if (ravelTemplate == null)
//            {
//                return new List<BoardAction>();
//            }


//            // Board[][] the wierd thing inside it to calculate the ofset of the template
//            return ravelTemplate.Apply(_platformRepository.Board[finalAgent.PositionX - ravelTemplate.FinalPositions.First().Value.x][finalAgent.PositionY - ravelTemplate.FinalPositions.First().Value.y].Id, time, 1);
//        }

//        return null;
//    }
    
//    public List<StateOld> GetExpandedStateOlds()
//    {

//        List<StateOld> expandedStateOlds = new List<StateOld>();

//        Dictionary<string, List<Types.RouteAction>> applicableActions = new Dictionary<string, List<Types.RouteAction>>();

//        foreach (string agentName in RoutableAgents)
//        {
//            Agent agent = Agents[agentName];
//            List<Types.RouteAction> possibleActions = Types.RouteAction.PossiblActions;
//            List<Types.RouteAction> agentActions = new List<Types.RouteAction>();

//            foreach (Types.RouteAction action in possibleActions)
//            {
//                if (agent.IsMoveApplicableOld(action, this))
//                {
//                    agentActions.Add(action);
//                }
//            }
//            applicableActions.Add(agentName, agentActions);
//        }

//        var jointActions = GetActionPermutations(applicableActions);


//        foreach (var jointAction in jointActions)
//        {
//            if (!IsConflicting(jointAction))
//            {
//                StateOld newStateOld = new StateOld(this, jointAction);
//                expandedStateOlds.Add(newStateOld);

//                Debugger.ExpandedStates += 1;
//            }
//        }

//        //Random random;

//        //random = Seed != null ? new Random((int)Seed) : new Random();

//        //expandedStateOlds = expandedStateOlds.OrderBy(x => random.Next()).ToList();

//        return expandedStateOlds;
//    }

//    private bool IsConflicting(Dictionary<string, Types.RouteAction> jointAction)
//    {
//        Dictionary<string, Tuple<int, int>> agentDestinations = new Dictionary<string, Tuple<int, int>>();
//        foreach (var action in jointAction)
//        {
//            var agent = Agents[action.Key];
//            agentDestinations.Add(action.Key, new Tuple<int, int>(agent.PositionX + action.Value.DropletXDelta, agent.PositionY + action.Value.DropletYDelta));
//        }

//        foreach (var action in jointAction)
//        {
//            if (action.Value.Type == Types.ActionType.NoOp)
//            {
//                continue;
//            }

//            foreach (var otherAction in jointAction)
//            {
//                if (action.Key == otherAction.Key || otherAction.Value.Type == Types.ActionType.NoOp)
//                {
//                    continue;
//                }

//                if (Math.Abs(agentDestinations[action.Key].Item1 - agentDestinations[otherAction.Key].Item1) <= 1 &&
//                    Math.Abs(agentDestinations[action.Key].Item2 - agentDestinations[otherAction.Key].Item2) <= 1)
//                {
//                    return true;
//                }
//            }


//        }

//        return false;
//    }

//    static List<Dictionary<string, Types.RouteAction>> GetActionPermutations(Dictionary<string, List<Types.RouteAction>> agentActions)
//    {
//        var agents = agentActions.Keys.ToList();

//        List<Dictionary<string, Types.RouteAction>> result = new List<Dictionary<string, Types.RouteAction>>();
//        GeneratePermutations(agentActions, new Dictionary<string, Types.RouteAction>(), agents, 0, result);
//        return result;
//    }

//    static void GeneratePermutations(
//        Dictionary<string, List<Types.RouteAction>> agentActions,
//        Dictionary<string, Types.RouteAction> current,
//        List<string> agents,
//        int depth,
//        List<Dictionary<string, Types.RouteAction>> result)
//    {
//        if (depth == agents.Count)
//        {
//            result.Add(new Dictionary<string, Types.RouteAction>(current));
//            return;
//        }
//        string agent = agents[depth];

//        foreach (var action in agentActions[agent])
//        {
//            current[agent] = action;

//            GeneratePermutations(agentActions, current, agents, depth + 1, result);
//        }
//    }


//    public int GetFScore()
//    {
//        return H + G;
//    }

//    private int CalculateHeuristic()
//    {
//        int h = 0;
//        foreach (IDropletCommand command in Commands)
//        {
//            if (command is Move moveCommand)
//            {
//                Agent agent = Agents[moveCommand.GetInputDroplets().First()];

//                if (!RoutableAgents.Contains(agent.DropletName)) continue;

//                int manhattanDistance = Math.Abs(moveCommand.PositionX - agent.PositionX) +
//                                        Math.Abs(moveCommand.PositionY - agent.PositionY);

//                // Penalize states where the path to the goal is blocked
//                if (IsPathBlocked(agent.PositionX, agent.PositionY, moveCommand.PositionX, moveCommand.PositionY, agent))
//                {
//                    manhattanDistance += 2;
//                }

//                // Penalize the act of standing still
//                if (manhattanDistance != 0 && JointAction != null && JointAction[agent.DropletName].Type == Types.ActionType.NoOp)
//                {
//                    manhattanDistance += 1;
//                }

//                h += manhattanDistance;

//            }
//            else
//            {
//                throw new InvalidOperationException("Trying to calculate heuristic for unknown dropletCommand!");
//            }
//        }
//        return h;
//    }

//    private bool IsPathBlocked(int startX, int startY, int endX, int endY, Agent agent, int maxDepth = 15)
//    {
//        int dx = Math.Abs(endX - startX);
//        int dy = -Math.Abs(endY - startY);
//        int sx = startX < endX ? 1 : -1;
//        int sy = startY < endY ? 1 : -1;
//        int err = dx + dy, e2;
//        int steps = 0;

//        while (true)
//        {
//            if (ContaminationMap[startX, startY] != 0 && ContaminationMap[startX, startY] != agent.SubstanceId)
//                return true;

//            if (startX == endX && startY == endY) break;
//            if (steps++ > maxDepth) break;

//            e2 = 2 * err;
//            if (e2 >= dy)
//            {
//                err += dy;
//                startX += sx;
//            }
//            if (e2 <= dx)
//            {
//                err += dx;
//                startY += sy;
//            }
//        }
//        return false;
//    }


//    public bool IsGoalStateOld()
//    {
//        foreach (var command in Commands)
//        {
//            if (!IsGoalStateOld(command))
//            {
//                return false;
//            }
//        }
//        return true;

//    }

//    public bool IsOneGoalStateOld()
//    {
//        foreach (var command in Commands)
//        {
//            if (IsGoalStateOld(command))
//            {
//                return true;
//            }
//        }
//        return false;

//    }

//    public bool IsPossibleEndStateOld()
//    {
//        bool isOneGoalStateOld = IsOneGoalStateOld();

//        if (!isOneGoalStateOld) return isOneGoalStateOld;
        

//        bool allGoalsReached = IsGoalStateOld();
//        if (allGoalsReached) return allGoalsReached;
        
//        //check that we dont terminate while we are in the process of unraveling a snake.
//        foreach (var agent in Agents)
//        {
//            if (agent.Value.SnakeBody.Count < agent.Value.GetMaximumSnakeLength())
//            {
//                return false;
//            }
//        }
//        return true;

//    }


//    public bool IsGoalStateOld(IDropletCommand dropletCommand)
//    {
//        switch (dropletCommand)
//        {
//            case Move moveCommand:
//                var agent = Agents[moveCommand.GetInputDroplets().First()];
//                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
//            default:
//                throw new InvalidOperationException("Trying to determine goalstate for unknown dropletCommand!");
//                break;
//        }

//    }

//    public static bool IsGoalStateOld(IDropletCommand dropletCommand, Agent agent)
//    {
//        switch (dropletCommand)
//        {
//            case Move moveCommand:
//                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
//            default:
//                throw new InvalidOperationException("Trying to determine goalstate for unknown dropletCommand!");
//                break;
//        }

//    }

//    public bool IsGoalStateOldReachable()
//    {
//        foreach (var command in Commands)
//        {
//            switch (command)
//            {
//                case Move moveCommand:
//                    var agent = Agents[moveCommand.GetInputDroplets().First()];
//                    //return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
//                    var goalPositionContamination = ContaminationMap[moveCommand.PositionX,moveCommand.PositionY];
//                    if (goalPositionContamination != 0 && goalPositionContamination != agent.SubstanceId)
//                    {
//                        throw new InvalidOperationException(
//                            $"Impossible for droplet {agent.DropletName} to reach the position in command {moveCommand}");
//                    }
//                    break;

//                default:
//                    break;
//            }
//        }

//        return true;
//    }


//    public override int GetHashCode()
//    {
//        if (_cachedHash != null)
//        {
//            return _cachedHash.Value;
//        }

//        int hash = 17;

//        //foreach (var value in ContaminationMap)
//        //{
//        //    hash = hash * 31 + value;
//        //}
//        //hash = hash * 31 + ContaminationMapHash;

//        foreach (var agent in Agents.Values)
//        {
//            hash = hash * 31 + agent.PositionX.GetHashCode();
//            hash = hash * 31 + agent.PositionY.GetHashCode();
//        }

//        _cachedHash = hash;
//        return hash;
//    }

//    public override bool Equals(object obj)
//    {
//        if (ReferenceEquals(this, obj))
//            return true;

//        if (obj is not StateOld otherStateOld)
//            return false;

//        if (!AreAgentsEqual(Agents, otherStateOld.Agents))
//            return false;

//        //if (!AreContaminationMapsEqual(ContaminationMap, otherStateOld.ContaminationMap))
//        //    return false;

//        return true;
//    }

//    //// TODO do we move this into the contamination service?
//    //private bool AreContaminationMapsEqual(byte[,] map1, byte[,] map2)
//    //{
//    //    if (map1.GetLength(0) != map2.GetLength(0) || map1.GetLength(1) != map2.GetLength(1))
//    //        return false;

//    //    for (int i = 0; i < map1.GetLength(0); i++)
//    //    {
//    //        for (int j = 0; j < map1.GetLength(1); j++)
//    //        {
//    //            if (map1[i, j] != map2[i, j])
//    //                return false;
//    //        }
//    //    }

//    //    return true;
//    //}

//    private bool AreAgentsEqual(Dictionary<string, Agent> agents1, Dictionary<string, Agent> agents2)
//    {
//        if (agents1.Count != agents2.Count)
//            return false;

//        foreach (var kvp in agents1)
//        {
//            if (!agents2.TryGetValue(kvp.Key, out var agent2))
//                return false;

//            var agent1 = kvp.Value;
//            if (agent1.PositionX != agent2.PositionX || agent1.PositionY != agent2.PositionY)
//                return false;
//        }

//        return true;
//    }
//}


