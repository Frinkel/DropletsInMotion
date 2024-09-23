﻿using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Controllers;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Functions;

namespace DropletsInMotion.Routers.Models;

public class State
{
    private int? Seed = null;

    public byte[,] ContaminationMap { get; private set; }
    public Dictionary<string, Agent> Agents { get; private set; }
    public int G { get; private set; }
    public State? Parent { get; private set; }
    public Dictionary<string, Types.RouteAction>? JointAction { get; private set; }

    private int H { get; set; }
    private List<string> RoutableAgents { get; set; }
    private List<ICommand> Commands { get; set; }
    private TemplateHandler _templateHandler;

    private int? CachedHash = null;
    private int ContaminationMapHash;

    // Initial state
    public State(List<string> routableAgents, Dictionary<string, Agent> agents, byte[,] contaminationMap, List<ICommand> commands, TemplateHandler templateHandler, int? seed = null)
    {
        Seed = seed;

        RoutableAgents = routableAgents;
        Agents = agents;
        ContaminationMap = contaminationMap;
        Commands = commands;
        JointAction = null;
        _templateHandler = templateHandler;

        Parent = null;
        G = 0;

        Seed = seed;
    }

    public State(State parent, Dictionary<string, Types.RouteAction> jointAction)
    {
        Seed = parent.Seed;

        Parent = parent;
        RoutableAgents = Parent.RoutableAgents;
        ContaminationMap = (byte[,]) Parent.ContaminationMap.Clone();
        Commands = Parent.Commands;
        JointAction = jointAction;
        _templateHandler = Parent._templateHandler;

        G = Parent.G + 1;

        // Clone parent agents
        Agents = new Dictionary<string, Agent>();
        foreach (var kvp in Parent.Agents)
        {
            Agents[kvp.Key] = (Agent) kvp.Value.Clone();
        }

        foreach (var actionKvp in jointAction)
        {
            Agent agent = Agents[actionKvp.Key];
            agent.Execute(actionKvp.Value);
            ContaminationMap = ApplicableFunctions.ApplyContamination(agent, ContaminationMap);
        }

        H = CalculateHeuristic();

    }

    public List<BoardAction> ExtractActions(double time)
    {
        List<State> chosenStates = new List<State>();
        State currentState = this;
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
                var agents = state.Parent.Agents;

                List<BoardAction> translatedActions = _templateHandler.ApplyTemplate(routeAction, agents[dropletName], currentTime);

                finalActions.AddRange(translatedActions);

            }


            finalActions = finalActions.OrderBy(b => b.Time).ToList();
            currentTime = finalActions.Last().Time;
        }

        //Console.WriteLine("______---------_________");
        //foreach (var action in finalActions)
        //{
        //    Console.WriteLine(action.ToString());
        //}
        return finalActions;

    }

    public List<State> GetExpandedStates(){

        List<State> expandedStates = new List<State>();

        Dictionary<string, List<Types.RouteAction>> applicableActions = new Dictionary<string, List<Types.RouteAction>>();
        
        foreach (string agentName in RoutableAgents)
        {
            Agent agent = Agents[agentName];
            List<Types.RouteAction> possibleActions = Types.RouteAction.PossiblActions;
            List<Types.RouteAction> agentActions = new List<Types.RouteAction>();

            foreach (Types.RouteAction action in possibleActions)
            {
                if (ApplicableFunctions.IsMoveApplicable(action, agent, this))
                {
                    agentActions.Add(action);
                }
            }
            applicableActions.Add(agentName,agentActions);
        }

        var jointActions = GetActionPermutations(applicableActions);


        foreach (var jointAction in jointActions)
        {
            if (!IsConflicting(jointAction))
            {
                State newState = new State(this, jointAction);
                expandedStates.Add(newState);
            }
        }

        Random random;

        random = Seed != null ? new Random((int)Seed) : new Random();

        expandedStates = expandedStates.OrderBy(x => random.Next()).ToList();

        return expandedStates;
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


    public int GetHeuristic()
    {
        return H*2 + G;
    }

    private int CalculateHeuristic()
    {
        int h = 0;
        foreach (ICommand command in Commands)
        {
            if (command is Move moveCommand)
            {
                Agent agent = Agents[moveCommand.GetInputDroplets().First()];

                int manhattanDistance = Math.Abs(moveCommand.PositionX - agent.PositionX) +
                                        Math.Abs(moveCommand.PositionY - agent.PositionY);

                // Penalize states where the path to the goal is blocked
                if (PathIsBlocked(agent.PositionX, agent.PositionY, moveCommand.PositionX, moveCommand.PositionY, agent))
                {
                    manhattanDistance += 10;
                }

                // Penalize the act of standing still
                if (manhattanDistance != 0 && JointAction[agent.DropletName].Type == Types.ActionType.NoOp)
                {
                    manhattanDistance += 5;
                }

                h += manhattanDistance;
            }
            else
            {
                throw new InvalidOperationException("Trying to calculate heuristic for unknown command!");
            }
        }
        return h;
    }

    private bool PathIsBlocked(int startX, int startY, int endX, int endY, Agent agent, int maxDepth = 15)
    {
        int dx = Math.Abs(endX - startX);
        int dy = -Math.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1;
        int sy = startY < endY ? 1 : -1;
        int err = dx + dy;
        int steps = 0;

        while (startX != endX || startY != endY)
        {
            // Early return if the current position is blocked
            if (ContaminationMap[startX, startY] != 0 && ContaminationMap[startX, startY] != agent.SubstanceId)
                return true;

            // Exit if maxDepth is reached
            if (++steps > maxDepth) break;

            int e2 = 2 * err;

            // Update coordinates based on error calculations
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

    //private bool PathIsBlocked(int startX, int startY, int endX, int endY, Agent agent, int maxDepth = 15)
    //{
    //    int dx = Math.Abs(endX - startX);
    //    int dy = -Math.Abs(endY - startY);
    //    int sx = startX < endX ? 1 : -1;
    //    int sy = startY < endY ? 1 : -1;
    //    int err = dx + dy, e2;
    //    int steps = 0;

    //    while (true)
    //    {
    //        if (ContaminationMap[startX, startY] != 0 && ContaminationMap[startX, startY] != agent.SubstanceId)
    //            return true;

    //        if (startX == endX && startY == endY) break;
    //        if (steps++ > maxDepth) break;

    //        e2 = 2 * err;
    //        if (e2 >= dy)
    //        {
    //            err += dy;
    //            startX += sx;
    //        }
    //        if (e2 <= dx)
    //        {
    //            err += dx;
    //            startY += sy;
    //        }
    //    }
    //    return false;
    //}


    // Helper method to check if an agent is near contamination
    private bool IsNearContamination(int x, int y, Agent agent)
    {
        // Check surrounding cells for contamination
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (x + dx >= 0 && x + dx < ContaminationMap.GetLength(0) &&
                    y + dy >= 0 && y + dy < ContaminationMap.GetLength(1) &&
                    ContaminationMap[x + dx, y + dy] != 0 && ContaminationMap[x + dx, y + dy] != agent.SubstanceId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Helper method to check if an agent is near another agent
    private bool IsNearOtherAgent(Agent agent)
    {
        foreach (var otherAgent in Agents.Values)
        {
            if (agent != otherAgent)
            {
                int distX = Math.Abs(agent.PositionX - otherAgent.PositionX);
                int distY = Math.Abs(agent.PositionY - otherAgent.PositionY);

                if (distX <= 2 && distY <= 2)
                {
                    return true;
                }
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


    public bool IsGoalState(ICommand command)
    {
        switch (command)
        {
            case Move moveCommand:
                var agent = Agents[moveCommand.GetInputDroplets().First()];
                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
            default:
                throw new InvalidOperationException("Trying to determine goalstate for unknown command!");
                break;
        }

    }


    public override int GetHashCode()
    {
        if (CachedHash != null)
        {
            return CachedHash.Value;
        }

        int hash = 17;

        foreach (var value in ContaminationMap)
        {
            hash = hash * 31 + value;
        }
        //hash = hash * 31 + ContaminationMapHash;

        foreach (var agent in Agents.Values)
        {
            hash = hash * 31 + agent.PositionX.GetHashCode();
            hash = hash * 31 + agent.PositionY.GetHashCode();
        }

        CachedHash = hash;
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

        if (!AreContaminationMapsEqual(ContaminationMap, otherState.ContaminationMap))
            return false;

        return true;
    }

    private bool AreContaminationMapsEqual(byte[,] map1, byte[,] map2)
    {
        if (map1.GetLength(0) != map2.GetLength(0) || map1.GetLength(1) != map2.GetLength(1))
            return false;

        for (int i = 0; i < map1.GetLength(0); i++)
        {
            for (int j = 0; j < map1.GetLength(1); j++)
            {
                if (map1[i, j] != map2[i, j])
                    return false;
            }
        }

        return true;
    }

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


