using DropletsInMotion.Controllers;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Routers.Functions;

namespace DropletsInMotion.Routers;
public class Router
{

    /*
     *  Actions:
     *  Move, SplitByVolume, SplitByRatio, Merge
     *
     *  Constraints:
     *  Single droplet routing
     *  
     */

    private Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();
    private Electrode[][] Board { get; set; }


    private byte[,] ContaminationMap { get; set; }


    private readonly MoveHandler _moveHandler;
    private readonly TemplateHandler _templateHandler;

    public Router(Electrode[][] board, Dictionary<string, Droplet> droplets)
    {
        Board = board;
        _templateHandler = new TemplateHandler(Board);
        _moveHandler = new MoveHandler(_templateHandler);
        ContaminationMap = new byte[Board.Length, Board[0].Length];


        foreach (var droplet in droplets)
        {
            Agent agent = new Agent(droplet.Value.DropletName, droplet.Value.PositionX, droplet.Value.PositionY, droplet.Value.Volume);
            Agents.Add(droplet.Key, agent);
            ContaminationMap = ApplicableFunctions.ApplyContamination(agent, ContaminationMap);
        }

        //PrintContaminationState();
    }

    public List<BoardAction> Route(Dictionary<string, Droplet> droplets, List<ICommand> commands, double time)
    {
        //foreach (var droplet in droplets)
        //{
        //    Agent agent = new Agent(droplet.Value.DropletName, droplet.Value.PositionX, droplet.Value.PositionY, droplet.Value.Volume);
        //    Agents.Add(droplet.Key, agent);
        //    ContaminationMap = ApplicableFunctions.ApplyContamination(agent, ContaminationMap);
        //}


        List<string> routableAgents = new List<string>();

        foreach (var command in commands)
        {
            routableAgents.AddRange(command.GetInputDroplets());
        }

        State s0 = new State(routableAgents, Agents, ContaminationMap, commands, _templateHandler);
        Frontier f = new Frontier();
        AstarRouter astarRouter = new AstarRouter();
        State sFinal = astarRouter.Search(s0, f, time);

        Agents = sFinal.Agents;
        ContaminationMap = sFinal.ContaminationMap;


        foreach (var agentKvp in Agents)
        {
            if (droplets.ContainsKey(agentKvp.Key))
            {
                var agent = agentKvp.Value;
                droplets[agentKvp.Key].PositionX = agent.PositionX;
                droplets[agentKvp.Key].PositionY = agent.PositionY;
            }
            else
            {
                Console.WriteLine($"Agent {agentKvp.Key} did NOT exist in droplets!");
            }
        }

        ApplicableFunctions.PrintContaminationState(sFinal.ContaminationMap);
        foreach (var agent in Agents)
        {
            Console.WriteLine(agent);

        }
        return sFinal.ExtractActions(time);
    }


    



    
}

