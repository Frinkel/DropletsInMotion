using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Atn;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Controllers;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Functions;
using DropletsInMotion.Routers.Models;
using static System.Collections.Specialized.BitVector32;

namespace DropletsInMotion.Routers.Models;

public class State
{

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
    public State(List<string> routableAgents, Dictionary<string, Agent> agents, byte[,] contaminationMap, List<ICommand> commands, TemplateHandler templateHandler)
    {
        RoutableAgents = routableAgents;
        Agents = agents;
        ContaminationMap = contaminationMap;
        Commands = commands;
        JointAction = null;
        _templateHandler = templateHandler;

        Parent = null;
        G = 0;
        //H = CalculateHeuristic();
        //Console.WriteLine($"Heuristic: {H+G}");
    }

    public State(State parent, Dictionary<string, Types.RouteAction> jointAction)
    {
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
        //Console.WriteLine(H);


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
        double currenTime = time;

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

                List<BoardAction> translatedActions = _templateHandler.ApplyTemplate(routeAction, agents[dropletName], currenTime);

                finalActions.AddRange(translatedActions);

            }
            finalActions = finalActions.OrderBy(b => b.Time).ToList();
            currenTime = finalActions.Last().Time;
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

        Random random = new Random();
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


    //private int CalculateHeuristic()
    //{
    //    int h = 0;
    //    foreach (ICommand command in Commands)
    //    {
    //        switch (command)
    //        {
    //            case Move moveCommand:
    //                int hTemp = 0;

    //                Agent agent = Agents[moveCommand.GetInputDroplets().First()];
    //                hTemp += Math.Abs(moveCommand.PositionX - agent.PositionX);
    //                hTemp += Math.Abs(moveCommand.PositionY - agent.PositionY);

    //                if (hTemp != 0 && JointAction[agent.DropletName].Type == Types.ActionType.NoOp)
    //                {
    //                    hTemp += 1;
    //                }

    //                h += hTemp;

    //                break;
    //            default:
    //                throw new InvalidOperationException("Trying to calculate heuristic for unknown command!");
    //                break;
    //        }
    //    }
    //    return h;
    //}

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
                int manhattanDistance = Math.Abs(moveCommand.PositionX - agent.PositionX) + Math.Abs(moveCommand.PositionY - agent.PositionY);

                // Check if path is blocked
                if (PathIsBlocked(agent.PositionX, agent.PositionY, moveCommand.PositionX, moveCommand.PositionY, agent))
                {
                    manhattanDistance += 10;  // Increase the heuristic by a penalty if the path is blocked
                }
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
        int err = dx + dy, e2;
        int steps = 0;

        while (true)
        {
            if (ContaminationMap[startX, startY] != 0 && ContaminationMap[startX, startY] != agent.SubstanceId)
                return true;

            if (startX == endX && startY == endY) break;
            if (steps++ > maxDepth) break; // Limit the lookahead depth

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

        return false; // No contamination within the lookahead limit
    }



    //private int CalculateHeuristic()
    //{
    //    int h = 0;

    //    // Iterate over each command to calculate the heuristic contribution
    //    foreach (ICommand command in Commands)
    //    {
    //        switch (command)
    //        {
    //            case Move moveCommand:
    //                int hTemp = 0;

    //                Agent agent = Agents[moveCommand.GetInputDroplets().First()];

    //                // Manhattan Distance to the goal
    //                hTemp += Math.Abs(moveCommand.PositionX - agent.PositionX);
    //                hTemp += Math.Abs(moveCommand.PositionY - agent.PositionY);


    //                // Penalize for NoOp actions
    //                if (hTemp != 0 && JointAction[agent.DropletName].Type == Types.ActionType.NoOp)
    //                {
    //                    hTemp += 10; // Penalize idling agents
    //                }

    //                // Additional heuristic penalties

    //                // Penalty for contamination proximity
    //                if (IsNearContamination(agent.PositionX, agent.PositionY, agent))
    //                {
    //                    hTemp += 2; // Increase penalty if near contamination
    //                }

    //                // Penalty for agent conflicts
    //                //if (IsNearOtherAgent(agent))
    //                //{
    //                //    hTemp += 3; // Penalize potential conflicts
    //                //}

    //                h += hTemp;
    //                break;

    //            default:
    //                throw new InvalidOperationException("Trying to calculate heuristic for unknown command!");
    //        }
    //    }

    //    return h;
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


    public override bool Equals(object obj)
    {
        //Console.WriteLine("We check");
        if (obj is State otherState)
        {

            //if (!ContaminationMap.Equals(otherState.ContaminationMap))
            //{
            //    Console.WriteLine("HTI");
            //}

            //if (AreContaminationMapsEqual(ContaminationMap, otherState.ContaminationMap))
            //{
            //    Console.WriteLine("Not equal"+ ContaminationMap.Equals(otherState.ContaminationMap));
            //}

            //if (!ContaminationMap.Equals(otherState.ContaminationMap))
            //{
            //    Console.WriteLine($"{!AreContaminationMapsEqual(ContaminationMap, otherState.ContaminationMap)} {!ContaminationMap.Equals(otherState.ContaminationMap)}");
            //    return false;
            //}
            if (!AreAgentsEqual(Agents, otherState.Agents))
            {
                //Console.WriteLine($"equals2 {false}");
                return false;
            }

            //// Compare contamination maps
            if (!AreContaminationMapsEqual(ContaminationMap, otherState.ContaminationMap))
            {
                //Console.WriteLine($"equals1 {false}");
                //Console.WriteLine("In here");
                return false;
            }
            //Console.WriteLine("Past");

            // Compare agent positions
            

            //Console.WriteLine($"equals3 {true}");
            //Console.WriteLine("They are equal");
            return true;
        }

        //Console.WriteLine($"equals4 {false}");
        return false;
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

        //foreach (var agent in Agents.Values)
        //{
        //    int x = agent.PositionX;
        //    int y = agent.PositionY;

        //    // Hash neighboring cells
        //    for (int i = Math.Max(0, x - 2); i <= Math.Min(ContaminationMap.GetLength(0) - 2, x + 2); i++)
        //    {
        //        for (int j = Math.Max(0, y - 2); j <= Math.Min(ContaminationMap.GetLength(1) - 2, y + 2); j++)
        //        {
        //            hash = hash * 31 + ContaminationMap[i, j];
        //        }
        //    }

        //    hash = hash * 31 + x.GetHashCode();
        //    hash = hash * 31 + y.GetHashCode();
        //}



        //foreach (var agent in Agents.Values)
        //{
        //    hash = hash * 31 + agent.PositionX.GetHashCode();
        //    hash = hash * 31 + agent.PositionY.GetHashCode();
        //}

        CachedHash = hash;
        return hash;
    }

    // TODO: Maybe this is fine
    //public override int GetHashCode()
    //{
    //    unchecked // Overflow is fine
    //    {
    //        const int fnvOffsetBasis = unchecked((int)2166136261);
    //        const int fnvPrime = 16777619;

    //        int hash = fnvOffsetBasis;

    //        // Hash contamination map using FNV-1a
    //        foreach (var value in ContaminationMap)
    //        {
    //            hash = (hash ^ value) * fnvPrime;
    //        }

    //        // Combine agent positions into the hash
    //        foreach (var agent in Agents.Values)
    //        {
    //            hash = (hash ^ agent.PositionX.GetHashCode()) * fnvPrime;
    //            hash = (hash ^ agent.PositionY.GetHashCode()) * fnvPrime;
    //        }

    //        return hash;
    //    }
    //}

    // Helper function to compare contamination maps
    private bool AreContaminationMapsEqual(byte[,] map1, byte[,] map2)
    {
        //if (map1.GetLength(0) != map2.GetLength(0) || map1.GetLength(1) != map2.GetLength(1))
        //    return false;

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

    // Helper function to compare agent positions
    private bool AreAgentsEqual(Dictionary<string, Agent> agents1, Dictionary<string, Agent> agents2)
    {
        // Check if the number of agents is the same
        if (agents1.Count != agents2.Count)
        {
            return false;
        }

        // Loop through each agent in agents1
        foreach (var key in agents1.Keys)
        {
            // Check if agents2 contains the same key
            if (!agents2.ContainsKey(key))
            {
                return false;
            }

            // Compare the positions of the agents with the same key
            var agent1 = agents1[key];
            var agent2 = agents2[key];

            if (agent1.PositionX != agent2.PositionX || agent1.PositionY != agent2.PositionY)
            {
                return false;
            }
        }

        return true;
    }

}


