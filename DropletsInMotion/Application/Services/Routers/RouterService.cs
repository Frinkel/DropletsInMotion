using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;

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

    //private Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();
    private Electrode[][] Board { get; set; }
    

    //private TemplateService _templateService;
    private readonly IContaminationService _contaminationService;
    private readonly ITemplateService _templateService;


    public RouterService(IContaminationService contaminationService, ITemplateService templateService)
    {
        _templateService = templateService;
        _contaminationService = contaminationService;


        //ApplicableFunctions.PrintContaminationState(ContaminationMap);
    }

    public void Initialize(Electrode[][] board)
    {
        Board = board;
        _templateService.Initialize(Board);



        //_templateService = new TemplateService(Board);

        //ContaminationMap = new byte[Board.Length, Board[0].Length];
        //Agents = agents;

        //foreach (var droplet in droplets)
        //{
        //    Agent agent = new Agent(droplet.Value.DropletName, droplet.Value.PositionX, droplet.Value.PositionY, droplet.Value.Volume);
        //    Agents.Add(droplet.Key, agent);
        //    ContaminationMap = _contaminationService.ApplyContamination(agent, ContaminationMap);
        //}

    }

    public List<BoardAction> Route(Dictionary<string, Agent> agents, List<ICommand> commands, byte[,] contaminationMap, double time, double? boundTime = null)
    {
        //Agents = agents;

        List<string> routableAgents = new List<string>();

        foreach (var command in commands)
        {
            routableAgents.AddRange(command.GetInputDroplets());
        }

        //ContaminationMap = contaminationMap;

        State s0 = new State(routableAgents, agents, contaminationMap, commands, _templateService, _contaminationService, Seed);
        Frontier f = new Frontier();
        AstarRouter astarRouter = new AstarRouter();
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
        //foreach (var agent in Agents)
        //{
        //    Console.WriteLine(agent);

        //}

        return sFinal.ExtractActions(time);
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

