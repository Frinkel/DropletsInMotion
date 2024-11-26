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
        AstarRouter astarRouter = new AstarRouter();

        List<State> commitedStates = new List<State>();
        State? sFinal = null;

        // Generate all permutations of the commands list
        var permutations = GetPermutations(commands, commands.Count);

        var chosenPermutation = permutations.First();

        foreach (var commandOrder in permutations)
        {
            
            // Clear commitedStates for each new permutation attempt
            commitedStates.Clear();
            sFinal = null;

            bool foundSolution = true;

            foreach (var command in commandOrder)
            {
                List<string> routableAgents = new List<string>();
                routableAgents.AddRange(command.GetInputDroplets());
                var reservedContaminationMap = _contaminationService.ReserveContaminations(commands, agents, (byte[,])contaminationMap.Clone());

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
                CombineStates(commitedStates, sFinal);

            }

            // If a solution was found for this permutation, break out of the loop
            if (foundSolution && sFinal != null)
            {
                chosenPermutation = commandOrder;
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
        //_contaminationService.PrintContaminationState(contaminationMap);
        //Console.WriteLine(time);
        return sFinal.ExtractActions(time);
    }

    public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => new T[] { t });

        return GetPermutations(list, length - 1)
            .SelectMany(t => list.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }

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


        int maxGInChosenStates = chosenStates.Count > 0 ? chosenStates[^1].G : -1; // Using C# 8.0 index from end operator


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
        oldState.ContaminationMap = CombineContaminationMaps(oldState.ContaminationMap, newState.ContaminationMap);
        
        oldState.Agents[newState.RoutableAgent] = newState.Agents[newState.RoutableAgent];

        foreach (var action in newState.JointAction)
        {
            oldState.JointAction[action.Key] = action.Value;
        }
    }

    private byte[,] CombineContaminationMaps(byte[,] map1, byte[,] map2)
    {
        int width = map1.GetLength(0);
        int height = map1.GetLength(1);
        byte[,] resultMap = new byte[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                byte v1 = map1[i, j];
                byte v2 = map2[i, j];

                if (v1 != 0 && v2 == 0)
                {
                    resultMap[i, j] = v1;
                }
                else if (v1 == 0 && v2 != 0)
                {
                    resultMap[i, j] = v2;
                }
                else if (v1 != 0 && v2 != 0)
                {
                    if (v1 == v2)
                    {
                        resultMap[i, j] = v1; 
                    }
                    else
                    {
                        resultMap[i, j] = 255;
                    }
                }
                else 
                {
                    resultMap[i, j] = 0;
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
                    //_contaminationService.PrintContaminationState(c1);
                    //_contaminationService.PrintContaminationState(c2);
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




}

