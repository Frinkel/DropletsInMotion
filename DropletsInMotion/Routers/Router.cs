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

    public List<BoardAction> Route(Dictionary<string, Droplet> droplets, List<ICommand> commands, double time, double? boundTime = null)
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
                    var agents = state.Parent.Agents;

                    List<BoardAction> translatedActions = _templateHandler.ApplyTemplate(routeAction, agents[dropletName], currentTime);

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
        if (Agents[inputDroplet1.DropletName].SubstanceId == Agents[inputDroplet2.DropletName].SubstanceId)
        {
            newAgent = new Agent(outputDroplet.DropletName, outputDroplet.PositionX, outputDroplet.PositionY, outputDroplet.Volume, Agents[inputDroplet1.DropletName].SubstanceId);
        }
        
        Agents.Add(outputDroplet.DropletName, newAgent);

        mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeHorizontal", outputDroplet, time));
        ApplicableFunctions.ApplyContaminationMerge(newAgent, ContaminationMap);

        return mergeActions;
    }

    public List<BoardAction> SplitByVolume(Dictionary<string, Droplet> droplets, SplitByVolume splitCommand, double time, int direction)
    {
        // Retrieve the input droplet
        Droplet inputDroplet = droplets[splitCommand.InputName]
                               ?? throw new InvalidOperationException($"No droplet found with name {splitCommand.InputName}.");

        Droplet outputDroplet1, outputDroplet2;
        string templateName;

        // Handle splitting based on direction
        switch (direction)
        {
            case 1: // Horizontal split, output 1 on the left (x-1) and output 2 on the right (x+1)
                outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX - 1,
                    inputDroplet.PositionY, inputDroplet.Volume - splitCommand.Volume);

                outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX + 1,
                    inputDroplet.PositionY, splitCommand.Volume);

                templateName = "splitHorizontal";
                break;

            case 3: // Horizontal split, but output 1 on the right (x+1) and output 2 on the left (x-1)
                outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX + 1,
                    inputDroplet.PositionY, inputDroplet.Volume - splitCommand.Volume);

                outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX - 1,
                    inputDroplet.PositionY, splitCommand.Volume);

                templateName = "splitHorizontal";
                break;

            case 2: // Vertical split, output 1 above (y-1) and output 2 below (y+1)
                outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX,
                    inputDroplet.PositionY - 1, inputDroplet.Volume - splitCommand.Volume);

                outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX,
                    inputDroplet.PositionY + 1, splitCommand.Volume);

                templateName = "splitVertical";
                break;

            case 4: // Vertical split, output 1 below (y+1) and output 2 above (y-1)
                outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX,
                    inputDroplet.PositionY + 1, inputDroplet.Volume - splitCommand.Volume);

                outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX,
                    inputDroplet.PositionY - 1, splitCommand.Volume);

                templateName = "splitVertical";
                break;

            default:
                throw new InvalidOperationException($"Invalid direction {direction}. Allowed values are 1, 2, 3, or 4.");
        }

        // Remove the input droplet and add the new droplets to the dictionary
        droplets.Remove(inputDroplet.DropletName);
        droplets[outputDroplet1.DropletName] = outputDroplet1;
        droplets[outputDroplet2.DropletName] = outputDroplet2;
        Agent newAgent1 = new Agent(outputDroplet1.DropletName, outputDroplet1.PositionX, outputDroplet1.PositionY, outputDroplet1.Volume, Agents[inputDroplet.DropletName].SubstanceId);
        Agent newAgent2 = new Agent(outputDroplet2.DropletName, outputDroplet2.PositionX, outputDroplet2.PositionY, outputDroplet2.Volume, Agents[inputDroplet.DropletName].SubstanceId);
        Agents.Remove(inputDroplet.DropletName);

        Agents.Add(outputDroplet1.DropletName, newAgent1);
        Agents.Add(outputDroplet2.DropletName, newAgent2);
        ApplicableFunctions.ApplyContamination(newAgent1, ContaminationMap);
        ApplicableFunctions.ApplyContamination(newAgent2, ContaminationMap);



        // Apply the appropriate template based on direction
        List<BoardAction> splitActions = new List<BoardAction>();
        splitActions.AddRange(_templateHandler.ApplyTemplate(templateName, inputDroplet, time));

        return splitActions;
    }





    public void UpdateContaminationMap(int x, int y, byte value)
    {
        ContaminationMap[x, y] = value;
    }
    



}

