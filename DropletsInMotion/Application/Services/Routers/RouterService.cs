using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using System;
using Antlr4.Runtime;
using System.Collections.Generic;

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


    public RouterService(IContaminationService contaminationService, ITemplateService templateService, IPlatformRepository platformRepository, ITemplateRepository templateRepository)
    {
        _templateService = templateService;
        _contaminationService = contaminationService;
        _platformRepository = platformRepository;
        _templateRepository = templateRepository;
    }

    public void Initialize(Electrode[][] board, int? seed = null)
    {
        Seed = seed;
        Board = board;
        _templateService.Initialize(Board);
    }

    public List<BoardAction> Route(Dictionary<string, Agent> agents, List<IDropletCommand> commands, byte[,] contaminationMap, double time, double? boundTime = null)
    {

        // CBS SEARCH

        // We need to add constraints to the goal of the extra nodes, otherwise they might move over them!

        HashSet<Constraint> initialConstraints = new HashSet<Constraint>();

        HashSet<Constraint> constraintSet;

        foreach (var dropletCommand in commands)
        {
            var command = (Move)dropletCommand;
            foreach (var agent in agents)
            {
                if (agent.Key == command.DropletName) continue;

                GenerateConstraintsWithSize(agent.Value, (command.PositionX, command.PositionY), agent.Value.GetAgentSize(), initialConstraints);
                //Console.WriteLine($"Added goal constraints for {agent.Key} for {command.DropletName}");

                //foreach (var con in goalConstraints)
                //{
                //    Console.WriteLine(con.Position);
                //}

                //Constraint constraint = new Constraint(agent.Key, (command.PositionX, command.PositionY));
                //Console.WriteLine(constraint.Agent);
                //Console.WriteLine((command.PositionX, command.PositionY));
                
                //initialConstraints.AddRange(goalConstraints);
            }
            
        }


        PrintConstraints(initialConstraints, contaminationMap.GetLength(0), contaminationMap.GetLength(1));
        

        Console.WriteLine($"Initial constraint count {initialConstraints.Count}");
        
        // Find initial routes
        Dictionary<string, State> routes = FindLowLevelSolutions(agents, commands, contaminationMap, initialConstraints);

        // Create initial node to CBS
        PriorityQueue<CbsNode, int> openNodes = new PriorityQueue<CbsNode, int>();
        CbsNode initialNode = new CbsNode(routes, initialConstraints, 0);

        Console.WriteLine($"Initial node score {initialNode.Score}");
        openNodes.Enqueue(initialNode, initialNode.Score);

        // Main CBS loop
        while (openNodes.Count != 0)
        {
            CbsNode bestNode = openNodes.Dequeue();
            Conflict? conflict = ValidateRoutes(bestNode.Routes);

            //Console.WriteLine("BEST NODE");
            //Console.WriteLine(bestNode.Constraints.Count);
            //foreach (var c in bestNode.Constraints)
            //{
            //    Console.WriteLine($"{c.Agent}  {c.Position}");
            //}

            if (conflict == null /*|| conflict.Count == 0*/)
            {
                Console.WriteLine("Solution with no conflicts found!");
                // Extract actions from the routes!

                foreach (var kvp in bestNode.Routes)
                {
                    State s = kvp.Value;
                    Console.WriteLine($"***************** {s.Agent} **********************");
                    _contaminationService.PrintContaminationState(s.ContaminationMap);
                }

                PrintConstraints(bestNode.Constraints, contaminationMap.GetLength(0), contaminationMap.GetLength(1));

                throw new Exception("YAY!");
            }  else if (bestNode.Depth == 15)
            {
                Console.WriteLine($"Constraints count {bestNode.Constraints.Count}");


                PrintConstraints(bestNode.Constraints, contaminationMap.GetLength(0), contaminationMap.GetLength(1));

                throw new Exception("Noooo");
            }

            // Create new nodes foreach agent
            //Console.WriteLine(conflict.Name);

            // We need to be able to return more than one constraint :(

            foreach (var kvp in conflict.Conflicts)
            {
                //Console.WriteLine(agent);
                //Constraint newConstraint = new Constraint(kvp.Key, kvp.Value, conflict.Time);

                HashSet<Constraint> newConstraints = new HashSet<Constraint>(bestNode.Constraints);
                //newConstraints.Add(newConstraint);
                GenerateConstraintsWithSize(agents[kvp.Key], kvp.Value, 1, newConstraints);
                
                //newConstraints.AddRange(sizeConstraints);

                Dictionary<string, State> newRoutes = new Dictionary<string, State>(bestNode.Routes);


                string constrainedAgentName = kvp.Key;
                Agent constrainedAgent = agents[constrainedAgentName];
                IDropletCommand command = commands.First(cmd => cmd.GetInputDroplets().Contains(constrainedAgentName));

                // Replan the path for the constrained agent
                State? sFinal = FindLowLevelSolutionForAgent(agents, constrainedAgent.DropletName, command, contaminationMap, newConstraints);

                if (sFinal == null)
                {
                    // No solution found; skip this child node
                    continue;
                }

                // Update the route for the constrained agent
                newRoutes[constrainedAgentName] = sFinal;






                //Dictionary<string, State>? newRoutes = FindLowLevelSolutions(agents, commands, contaminationMap, newConstraints);




                //if (newRoutes == null)
                //{
                //    // A route could not be found therefore this configuration is unsolvable?
                //    Console.WriteLine("Unsolveable");
                //    continue;
                //}

                //foreach (var kvp in agents)
                //{
                //    Console.WriteLine(kvp.Key);
                //    Console.WriteLine((kvp.Value.PositionX, kvp.Value.PositionY));
                //}

                
                CbsNode newNode = new CbsNode(newRoutes, newConstraints, bestNode.Depth + 1);
                openNodes.Enqueue(newNode, newNode.Score);
            }

        }

        throw new Exception("We did not fing a solution..");
        return null;
       
    }

    public Conflict? ValidateRoutes(Dictionary<string, State> routes)
    {
        List<Conflict> foundConflicts = new List<Conflict>();

        Dictionary<string, List<State>> agentPaths = new Dictionary<string, List<State>>();
        foreach (var kvp in routes)
        {
            string agentName = kvp.Key;
            State finalState = kvp.Value;

            List<State> path = new List<State>();
            State state = finalState;

            while (state != null)
            {
                path.Add(state);
                state = state.Parent;
            }

            path.Reverse();
            agentPaths[agentName] = path;
        }

        foreach (var kvp in agentPaths)
        {
            string currentAgentName = kvp.Key;
            List<State> currentStates = kvp.Value;


            foreach (var otherKvp in agentPaths)
            {
                string otherAgentName = otherKvp.Key;

                if (currentAgentName.Equals(otherAgentName)) continue;

                List<State> otherStates = otherKvp.Value;

                for (int i = 0; i < currentStates.Count; i++)
                {
                    State currentState = currentStates[i];

                    // Ensure we get the correct state if it has less states that the current
                    State otherState = otherStates.Count > i ? otherStates[i] : otherStates.Last();

                    Agent currentAgent = currentState.Agents[currentAgentName];
                    Agent otherAgent = otherState.Agents[otherAgentName];

                    bool contaminationCollision = AreContaminationsConflicting(currentState, otherState, currentAgent);
                    if (contaminationCollision) {
                        //Console.WriteLine("Collision in contamination");
                        
                        //Conflict conflict = new Conflict($"Contamination conflict at {currentAgent.PositionX} {currentAgent.PositionY}", 
                        //    new List<Agent>() {currentAgent, otherAgent}, (currentAgent.PositionX, currentAgent.PositionY));

                        Dictionary<string, (int x, int y)> conflicts = new Dictionary<string, (int x, int y)>();
                        conflicts.Add(currentAgentName, (currentAgent.PositionX, currentAgent.PositionY));
                        conflicts.Add(otherAgentName, (otherAgent.PositionX, otherAgent.PositionY));

                        Conflict conflict = new Conflict($"Contamination conflict at {currentAgent.PositionX} {currentAgent.PositionY}", conflicts);


                        //Console.WriteLine(conflict.Name);
                        
                        return conflict;
                        //foundConflicts.Add(conflict);
                    }


                    //bool agentCollision = AreAgentsConflicting(currentAgent, otherAgent);
                    // CHECK COLLISION

                }
            }
        }

        return null; //foundConflicts;
    }

    // Find the low level routes/solutions for the cbs
    public Dictionary<string, State>? FindLowLevelSolutions(Dictionary<string, Agent> agents, List<IDropletCommand> commands, byte[,] contaminationMap, HashSet<Constraint> constraints)
    {
        AstarRouter astarRouter = new AstarRouter();
        Dictionary<string, State> routes = new Dictionary<string, State>();

        foreach (var command in commands)
        {
            if (!(command is Move)) throw new Exception("Tried to route a non move command");

            var agent = command.GetInputDroplets().First();

            State s0 = new State(agents, agent, contaminationMap, constraints, command, _contaminationService, _platformRepository, _templateRepository);
            Frontier f = new Frontier();

            State? sFinal = astarRouter.Search(s0, f);

            if (sFinal == null) return null; //throw new Exception($"The goal was unreachable for agent {agent}"); // return new List<BoardAction>();

            routes.Add(agent, sFinal);

            //sFinal.ExtractActions(0);

            //_contaminationService.PrintContaminationState(sFinal.ContaminationMap);
        }

        return routes;
    }

    public State? FindLowLevelSolutionForAgent(Dictionary<string, Agent> agents, string agent, IDropletCommand command, byte[,] contaminationMap, HashSet<Constraint> constraints)
    {
        AstarRouter astarRouter = new AstarRouter();

        State s0 = new State(agents, agent, contaminationMap, constraints, command, _contaminationService, _platformRepository, _templateRepository);
        Frontier f = new Frontier();

        State? sFinal = astarRouter.Search(s0, f);

        return sFinal;
    }


    public void GenerateConstraintsWithSize(Agent agent, (int x, int y) position, int size, HashSet<Constraint> constraints)
    {
        int x = position.x;
        int y = position.y;

        //int size = agent.GetAgentSize();

        // Loop over the area of the agent's goal position, size x size
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int posX = x + i;
                int posY = y + j;

                // Create constraints for this position
                constraints.Add(new Constraint(agent.DropletName, (posX, posY))); // 'null' indicates the constraint applies to all agents
            }
        }

        // Apply constraints to the neighbors around the agent's goal area
        for (int i = -1; i <= size; i++)
        {
            for (int j = -1; j <= size; j++)
            {
                if (i >= 0 && i < size && j >= 0 && j < size)
                {
                    // Skip the internal goal area cells that are already constrained
                    continue;
                }

                int posX = x + i;
                int posY = y + j;

                // Create constraints for the neighboring positions
                constraints.Add(new Constraint(agent.DropletName, (posX, posY)));
            }
        }

        //return initialConstraints;
    }


    public bool AreContaminationsConflicting(State currentState, State otherState, Agent currentAgent)
    {

        int actionX = Math.Clamp(currentAgent.PositionX , 0, otherState.ContaminationMap.GetLength(0) - 1);
        int actionY = Math.Clamp(currentAgent.PositionY , 0, otherState.ContaminationMap.GetLength(1) - 1);

        var contamination = otherState.ContaminationMap[actionX, actionY];
        if (contamination != 0 && contamination != currentAgent.SubstanceId)
        {
            //Console.WriteLine($"Conflict at {actionX} {actionY} {currentAgent.DropletName}  {contamination}");
            return true;
        }

        return false;
    }

    public bool AreContaminationsConflictingold(State currentState, State otherState, Agent currentAgent)
    {
        int x = currentAgent.PositionX;
        int y = currentAgent.PositionY;

        int mapWidth = otherState.ContaminationMap.GetLength(0);
        int mapHeight = otherState.ContaminationMap.GetLength(1);

        // Define the relative positions to check (current cell and adjacent cells)
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                int newX = x + offsetX;
                int newY = y + offsetY;

                // Check bounds to prevent IndexOutOfRangeException
                if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
                {
                    continue;
                }

                // Get the contamination at the neighboring position
                var contamination = otherState.ContaminationMap[newX, newY];

                // Check if the contamination is from a different substance
                if (contamination != 0 && contamination != currentAgent.SubstanceId)
                {
                    Console.WriteLine($"Conflict at {newX} {newY} {currentAgent.DropletName}  {contamination}");
                    // Conflict detected
                    return true;
                }
            }
        }

        // No conflict detected
        return false;
    }

    public bool AreAgentsConflicting(Agent currentAgent, Agent otherAgent)
    {
        Console.WriteLine($"Snake length {otherAgent.SnakeBody.Count}");
        foreach (var pos in otherAgent.SnakeBody)
        {
            Console.WriteLine($"{otherAgent.SubstanceId} {currentAgent.SubstanceId}");

            // TODO: Maybe we should return, for now we let it create all constraints
            //if (otherAgent.SubstanceId != currentAgent.SubstanceId) return false;     // throw new Exception($"Agent has different substance ids but are closer than contamination should allow. Agent {currentAgentName} with substance id {currentAgent.SubstanceId} and agent {otherAgentName} with substance id {otherAgent.SubstanceId}. \nAt position {currentAgent.PositionX}, {currentAgent.PositionY}");

            int diffX = Math.Abs(pos.x - currentAgent.PositionX);
            int diffY = Math.Abs(pos.y - currentAgent.PositionY);

            if (diffX <= 1 && diffY <= 1)
            {
                Console.WriteLine($"Agent {currentAgent.DropletName} too close to snake body of {otherAgent.DropletName} at position ({currentAgent.PositionX}, {currentAgent.PositionY})");
                return true;
            }
        }

        return false;
    }


    private bool ConflictingSates(State s1, State s2)
    {
        byte[,] c1 = s1.ContaminationMap;
        byte[,] c2 = s2.ContaminationMap;

        for (int i = 0; i < c1.GetLength(0); i++)
        {
            for (int j = 0; j < c1.GetLength(1); j++)
            {
                if (c1[i, j] != 0 && c2[i, j] != 0 && c1[i, j] != c2[i, j])
                {
                    Console.WriteLine($"Conflict at {i}, {j}");
                    _contaminationService.PrintContaminationState(c1);
                    _contaminationService.PrintContaminationState(c2);
                    return true;
                }
            }
        }

        return false;
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


    public void PrintConstraints(HashSet<Constraint> constraints, int gridWidth, int gridHeight)
    {
        // Create a HashSet for faster lookup of constrained positions
        var constraintDict = constraints.GroupBy(c => c.Position)
                                        .ToDictionary(g => g.Key, g => g.Select(c => c.Agent).ToList());

        // Assign colors to agents
        var agentColors = new Dictionary<string, ConsoleColor>();
        var availableColors = new List<ConsoleColor>
            {
                ConsoleColor.Blue,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Magenta,
                ConsoleColor.Yellow,
                ConsoleColor.Red,
            };
        int colorIndex = 0;

        foreach (var agent in constraints.Select(c => c.Agent).Distinct())
        {
            agentColors[agent] = availableColors[colorIndex % availableColors.Count];
            colorIndex++;
        }

        // Determine the maximum number of digits for proper alignment
        int maxDigits = 2;

        // Loop over the grid positions
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (constraintDict.TryGetValue((x, y), out var agents))
                {
                    // Get the first agent (or handle multiple agents if needed)
                    string agent = agents.FirstOrDefault() ?? "X";
                    ConsoleColor color = agentColors.ContainsKey(agent) ? agentColors[agent] : ConsoleColor.Blue;

                    // Set the color for the agent
                    Console.ForegroundColor = color;

                    // Print the agent's initial or symbol
                    string symbol = agent.Length > 0 ? agent.Substring(0, 2) : "X";
                    Console.Write(symbol.PadLeft(maxDigits) + " ");
                }
                else
                {
                    // Print a dot or space for empty positions
                    Console.Write(".".PadLeft(maxDigits) + " ");
                }

                // Reset color after printing
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

}








// TODO: OLD CODE MIGHT USE LATER IN  ROUTE

//_contaminationService.PrintContaminationState(contaminationMap);




//if (boundTime != null)
//{
//    List<State> chosenStates = new List<State>();
//    State currentState = sFinal;
//    while (currentState.Parent != null)
//    {
//        chosenStates.Add(currentState);
//        currentState = currentState.Parent;
//    }

//    chosenStates = chosenStates.OrderBy(s => s.G).ToList();

//    List<BoardAction> finalActions = new List<BoardAction>();
//    double currentTime = time;

//    foreach (State state in chosenStates)
//    {
//        foreach (var actionKvp in state.JointAction)
//        {
//            if (actionKvp.Value == Types.RouteAction.NoOp)
//            {
//                continue;
//            }
//            string dropletName = actionKvp.Key;
//            string routeAction = actionKvp.Value.Name;
//            var parentAgents = state.Parent.Agents;

//            List<BoardAction> translatedActions = _templateService.ApplyTemplate(routeAction, parentAgents[dropletName], currentTime);

//            finalActions.AddRange(translatedActions);

//        }

//        finalActions = finalActions.OrderBy(b => b.Time).ToList();
//        currentTime = finalActions.Last().Time;
//        if (currentTime >= boundTime)
//        {
//            sFinal = state;
//            break;
//        }
//    }
//}

//_contaminationService.CopyContaminationMap(sFinal.ContaminationMap, contaminationMap);


//foreach (var agentKvp in sFinal.Agents)
//{
//    if (agents.ContainsKey(agentKvp.Key))
//    {
//        var agent = agentKvp.Value;
//        agents[agentKvp.Key].PositionX = agent.PositionX;
//        agents[agentKvp.Key].PositionY = agent.PositionY;
//        agents[agentKvp.Key].SnakeBody = agent.SnakeBody;
//    }
//    else
//    {
//        Console.WriteLine($"Agent {agentKvp.Key} did NOT exist in droplets!");
//    }
//}

//foreach (var actionKvp in sFinal.JointAction)
//{
//    string dropletName = actionKvp.Key;
//    Agent agent = sFinal.Agents[dropletName];

//    IDropletCommand dropletCommand =
//        sFinal.Commands.Find(c => c.GetInputDroplets().First() == dropletName);

//    if (State.IsGoalState(dropletCommand, agent))
//    {
//        _contaminationService.ApplyContaminationWithSize(agent, contaminationMap);
//    }
//}

////routableAgents.ForEach(agent => _contaminationService.ApplyContaminationWithSize(agents[agent], contaminationMap));

//_contaminationService.PrintContaminationState(contaminationMap);

//return sFinal.ExtractActions(time);