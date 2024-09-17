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

namespace DropletsInMotion.Routers.Models;

public class State
{
    public int Heuristic { get; set; }
    private int H { get; set; }
    private int G { get; set; }
    private byte[,] ContaminationMap { get; set; }
    private State? Parent { get; set; }

    private List<string> RoutableAgents { get; set; }
    private Dictionary<string, Agent> Agents { get; set; }
    private List<ICommand> Commands { get; set; }

    // Initial state
    public State(List<string> routableAgents, Dictionary<string, Agent> agents, byte[,] contaminationMap, List<ICommand> commands)
    {
        RoutableAgents = routableAgents;
        Agents = agents;
        ContaminationMap = contaminationMap;
        Commands = commands;

        Parent = null;
        G = 0;
    }

    public State(State parent, List<Tuple<string, Types.RouteAction>> jointAction)
    {
        Parent = parent;
        RoutableAgents = Parent.RoutableAgents;
        ContaminationMap = (byte[,]) Parent.ContaminationMap.Clone();
        Commands = Parent.Commands;

        G = Parent.G + 1;

        // Clone parent agents
        Agents = new Dictionary<string, Agent>();
        foreach (var kvp in Parent.Agents)
        {
            Agents[kvp.Key] = (Agent) kvp.Value.Clone();
        }

        foreach (var actionTuple in jointAction)
        {
            Agent agent = Agents[actionTuple.Item1];
            agent.Execute(actionTuple.Item2);
            ContaminationMap = ApplicableFunctions.ApplyContamination(agent, ContaminationMap);
        }
    }
}

