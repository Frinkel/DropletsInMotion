using System.Reflection.Metadata.Ecma335;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;

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


        //List<State> finalStates = new List<State>();
        //List<State> conflictingStates = new List<State>();



        //foreach (var command in commands)
        //{
        //    List<string> routableAgents1 = new List<string>(command.GetInputDroplets());
        //    State state = new State(routableAgents1, agents, contaminationMap, new List<IDropletCommand>() { command }, _templateService, _contaminationService, Seed);
        //    Frontier f1 = new Frontier();
        //    State sFinal1 = astarRouter.Search(state, f1);
        //    finalStates.Add(sFinal1);
        //}

        //foreach (var finalState1 in finalStates)
        //{
        //    foreach (var finalState2 in finalStates)
        //    {
        //        if (finalState2 != finalState1)
        //        {
        //            if (ConflictingSates(finalState1, finalState2))
        //            {
        //                conflictingStates.Add(finalState1);
        //                conflictingStates.Add(finalState2);
        //            }
        //        }
        //    }
        //}

        //if (conflictingStates.Count == 0)
        //{
        //    Console.WriteLine("No conflicting states found!");
        //}
        //else
        //{
        //    conflictingStates.ForEach(s => Console.WriteLine(s));
        //}














        List<string> routableAgents = new List<string>();

        foreach (var command in commands)
        {
            routableAgents.AddRange(command.GetInputDroplets());
        }

        State s0 = new State(routableAgents, agents, contaminationMap, commands, _templateService, _contaminationService, _platformRepository, _templateRepository, Seed);
        Frontier f = new Frontier();
        //AstarRouter astarRouter = new AstarRouter();
        State sFinal = astarRouter.Search(s0, f);


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

                    List<BoardAction> translatedActions = _templateService.ApplyTemplate(routeAction, parentAgents[dropletName], currentTime);

                    finalActions.AddRange(translatedActions);

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
            }
            else
            {
                Console.WriteLine($"Agent {agentKvp.Key} did NOT exist in droplets!");
            }
        }

        _contaminationService.PrintContaminationState(contaminationMap);

        routableAgents.ForEach(agent => _contaminationService.ApplyContaminationWithSize(agents[agent], contaminationMap));
        return sFinal.ExtractActions(time);
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




}

