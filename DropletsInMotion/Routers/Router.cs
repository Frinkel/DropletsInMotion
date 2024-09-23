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

    public int? Seed = null;

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

        //ApplicableFunctions.PrintContaminationState(ContaminationMap);
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

        State s0 = new State(routableAgents, Agents, ContaminationMap, commands, _templateHandler, Seed);
        Frontier f = new Frontier();
        AstarRouter astarRouter = new AstarRouter();
        State sFinal = astarRouter.Search(s0, f);
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

    public List<BoardAction> Merge(Dictionary<string, Droplet> droplets, Merge mergeCommand, double time)
    {
        // Add logic for processing the Merge command
        //Console.WriteLine($"Merging droplets with IDs: {mergeCommand.InputName1}, {mergeCommand.InputName2}");


        //Merge
        Droplet inputDroplet1 = droplets[mergeCommand.InputName1]
                                ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");

        Droplet inputDroplet2 = droplets[mergeCommand.InputName2]
                                ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");


        List<BoardAction> mergeActions = new List<BoardAction>();
        Droplet outputDroplet = new Droplet(mergeCommand.OutputName, mergeCommand.PositionX, mergeCommand.PositionY,
            inputDroplet1.Volume + inputDroplet2.Volume);

        //check that droplets are not more than 1 away from merge position
        if (Math.Abs(inputDroplet1.PositionX - mergeCommand.PositionX) > 1 
            || Math.Abs(inputDroplet1.PositionY - mergeCommand.PositionY) > 1
            || Math.Abs(inputDroplet2.PositionX - mergeCommand.PositionX) > 1
            || Math.Abs(inputDroplet2.PositionY - mergeCommand.PositionY) > 1)
        {
            throw new InvalidOperationException("Droplets is not in position to merge they are too far");
        }

        if (Math.Abs(inputDroplet1.PositionX - inputDroplet2.PositionX) == 2 && inputDroplet1.PositionY == inputDroplet2.PositionY)
        {
            mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeHorizontal", outputDroplet, time));

        }
        else if (Math.Abs(inputDroplet1.PositionY - inputDroplet2.PositionY) == 2 && inputDroplet1.PositionX == inputDroplet2.PositionX)
        {
            mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeVertical", outputDroplet, time));
        }
        else
        {
            throw new InvalidOperationException("Droplets are not in position to merge");
        }

        Agents.Remove(inputDroplet1.DropletName);
        Agents.Remove(inputDroplet2.DropletName);
        droplets.Remove(inputDroplet1.DropletName);
        droplets.Remove(inputDroplet2.DropletName);
        droplets[outputDroplet.DropletName] = outputDroplet;
        Agent newAgent = new Agent(outputDroplet.DropletName, outputDroplet.PositionX, outputDroplet.PositionY, outputDroplet.Volume);
        Agents.Add(outputDroplet.DropletName, newAgent);

        mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeHorizontal", outputDroplet, time));
        ApplicableFunctions.ApplyContaminationMerge(newAgent, ContaminationMap);

        return mergeActions;
    }

    //public List<BoardAction> SplitByVolume(Dictionary<string, Droplet> droplets, Merge mergeCommand, double time)
    //{
    //    // Add logic for processing the SplitByRatio command
    //    //Console.WriteLine($"Splitting droplet with ratio {splitByRatioCommand.Ratio}");


    //    Droplet inputDroplet = Droplets[splitByRatioCommand.InputName]
    //                           ?? throw new InvalidOperationException($"No droplet found with name {splitByRatioCommand.InputName}.");


    //    // Create the new droplets
    //    Droplet outputDroplet1 = new Droplet(splitByRatioCommand.OutputName1, inputDroplet.PositionX - 1,
    //        inputDroplet.PositionY, inputDroplet.Volume * (1 - splitByRatioCommand.Ratio));

    //    Droplet outputDroplet2 = new Droplet(splitByRatioCommand.OutputName2, inputDroplet.PositionX + 1,
    //        inputDroplet.PositionY, inputDroplet.Volume * splitByRatioCommand.Ratio);

    //    Droplets.Remove(inputDroplet.DropletName);
    //    Droplets[outputDroplet1.DropletName] = outputDroplet1;
    //    Droplets[outputDroplet2.DropletName] = outputDroplet2;

    //    List<BoardAction> splitActions = new List<BoardAction>();

    //    splitActions.AddRange(_templateHandler.ApplyTemplate("splitHorizontal", inputDroplet, Time));

    //    double time1 = splitActions.Any() ? splitActions.Last().Time : Time;
    //    double time2 = splitActions.Any() ? splitActions.Last().Time : Time;
    //    splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet1, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1, ref time1));
    //    splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet2, splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2, ref time2));

    //    return splitActions;
    //}




    public void UpdateContaminationMap(int x, int y, byte value)
    {
        ContaminationMap[x, y] = value;
    }
    



}

