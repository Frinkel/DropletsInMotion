﻿using DropletsInMotion.Controllers;
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
        //foreach (var agent in Agents)
        //{
        //    Console.WriteLine(agent);

        //}

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

        int outPutDropletX = (inputDroplet1.PositionX + inputDroplet2.PositionX) / 2;
        int outPutDropletY = (inputDroplet1.PositionY + inputDroplet2.PositionY) / 2;
        Droplet outputDroplet = new Droplet(mergeCommand.OutputName, outPutDropletX, outPutDropletY,
            inputDroplet1.Volume + inputDroplet2.Volume);

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


        Agent newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume);

        if (Agents[inputDroplet1.DropletName].SubstanceId == Agents[inputDroplet2.DropletName].SubstanceId)
        {
            newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume, Agents[inputDroplet1.DropletName].SubstanceId);
        }

        Agents.Remove(inputDroplet1.DropletName);
        Agents.Remove(inputDroplet2.DropletName);
        droplets.Remove(inputDroplet1.DropletName);
        droplets.Remove(inputDroplet2.DropletName);
        droplets[newAgent.DropletName] = newAgent;

        
        Agents.Add(newAgent.DropletName, newAgent);

        mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeHorizontal", newAgent, time));
        ApplicableFunctions.ApplyContaminationMerge(newAgent, ContaminationMap);
        ApplicableFunctions.PrintContaminationState(ContaminationMap);
        Console.WriteLine(outputDroplet);
        return mergeActions;
    }

    public List<BoardAction> SplitByVolume(Dictionary<string, Droplet> droplets, SplitByVolume splitCommand, double time, int direction)
    {
        // Retrieve the input droplet
        Droplet inputDroplet = droplets[splitCommand.InputName]
                               ?? throw new InvalidOperationException($"No droplet found with name {splitCommand.InputName}.");

        if (droplets.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
        {
            throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName1} already exists.");
        }
        if (droplets.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
        {
            throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName2} already exists.");
        }
        if (splitCommand.OutputName2 == splitCommand.OutputName1)
        {
            throw new InvalidOperationException($"Droplet with the same names can not be split.");
        }

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

        Agents[outputDroplet1.DropletName] = newAgent1;
        Agents[outputDroplet2.DropletName] = newAgent2;
        ApplicableFunctions.ApplyContamination(newAgent1, ContaminationMap);
        ApplicableFunctions.ApplyContamination(newAgent2, ContaminationMap);



        // Apply the appropriate template based on direction
        List<BoardAction> splitActions = new List<BoardAction>();
        splitActions.AddRange(_templateHandler.ApplyTemplate(templateName, inputDroplet, time));

        return splitActions;
    }

    public List<BoardAction> Mix(Dictionary<string, Droplet> droplets, Mix mixCommand, double compilerTime)
    {
        Agent inputDroplet = Agents[mixCommand.DropletName]
                               ?? throw new InvalidOperationException($"No droplet found with name {mixCommand.DropletName}.");
        if (ApplicableFunctions.IsAreaContaminated(ContaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX,
                mixCommand.PositionY, mixCommand.Width, mixCommand.Height))
        {
            throw new InvalidOperationException($"Mix not possible Area is contaminated.");
        }

        List<BoardAction> mixActions = new List<BoardAction>();

        double time1 = compilerTime;

        for (int i = 0; i < mixCommand.RepeatTimes; i++)
        {
            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX + mixCommand.Width, inputDroplet.PositionY, ref time1));
            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY + mixCommand.Height, ref time1));
            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX - mixCommand.Width, inputDroplet.PositionY, ref time1));
            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY - mixCommand.Height, ref time1));
        }
        Console.WriteLine("-----------------------------------------------------");
        ApplicableFunctions.PrintContaminationState(ContaminationMap);
        ApplicableFunctions.UpdateContaminationArea(ContaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX-1,
            mixCommand.PositionY-1, mixCommand.Width+2, mixCommand.Height+2);
        ApplicableFunctions.PrintContaminationState(ContaminationMap);
        return mixActions;
    }



    // USED ONLY FOR TEST
    public void UpdateAgentSubstanceId(string agent, byte substanceId)
    {
        Agents[agent].SubstanceId = substanceId;
        ContaminationMap = ApplicableFunctions.ApplyContaminationMerge(Agents[agent], ContaminationMap);

    }
    // USED ONLY FOR TEST
    public byte GetAgentSubstanceId(string agent)
    {
        return Agents[agent].SubstanceId;
    }
    public byte[,] GetContaminationMap()
    {
        return ContaminationMap;
    }
    public Dictionary<string, Agent> GetAgents()
    {
        return Agents;
    }
    // USED ONLY FOR TEST
    public void UpdateContaminationMap(int x, int y, byte value)
    {
        ContaminationMap[x, y] = value;
    }
    



}

