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
    private int H { get; set; }
    private int G { get; set; }
    private byte[,] ContaminationMap { get; set; }
    private State? Parent { get; set; }

    private List<string> RoutableAgents { get; set; }
    private Dictionary<string, Agent> Agents { get; set; }
    private List<ICommand> Commands { get; set; }
    private Dictionary<string, Types.RouteAction>? JointAction { get; set; }

    private TemplateHandler _templateHandler;

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


    }

    public int GetHeuristic()
    {
        return H + G;
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
                if (ApplicableFunctions.IsMoveApplicable(action, agent, Agents, ContaminationMap))
                {
                    agentActions.Add(action);
                }
            }
            applicableActions.Add(agentName,agentActions);
        }


        return expandedStates;
    }


    private int CalculateHeuristic()
    {
        int h = 0;
        foreach (ICommand command in Commands)
        {
            switch (command)
            {
                case Move moveCommand:
                    Agent agent = Agents[moveCommand.GetInputDroplets().First()];
                    h +=  Math.Abs(moveCommand.PositionX - agent.PositionX);
                    h += Math.Abs(moveCommand.PositionY - agent.PositionY);
                    break;
                default:
                    throw new InvalidOperationException("Trying to calculate heuristic for unknown command!");
                    break;
            }
        }
        return h;
    }

    public bool IsGoalState(ICommand command, Dictionary<string, Agent> agents)
    {
        switch (command)
        {
            case Move moveCommand:
                var agent = agents[moveCommand.GetInputDroplets().First()];
                return agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY;
            default:
                throw new InvalidOperationException("Trying to calculate heuristic for unknown command!");
                break;
        }
        
    }
}

