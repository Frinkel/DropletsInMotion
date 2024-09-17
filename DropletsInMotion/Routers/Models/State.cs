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
    private byte[,] Contamination { get; set; }
    private Dictionary<string, Agent> Agents { get; set; }
    private State? Parent { get; set; }
    private Dictionary<ICommand, Types.RouteAction> RouteActions { get; set; }
    private List<ICommand> Commands { get; set; }


    private readonly TemplateHandler _templateHandler;

    private double Time { get; set; }

    public State(byte[,] contamination, Dictionary<string, Agent> agents, double time, List<ICommand> commands, TemplateHandler templateHandler)
    {
        Contamination = contamination;
        Agents = agents;
        Time = time;
        Commands = commands;

        _templateHandler = templateHandler;

        RouteActions = null;
        Parent = null;

        G = 0;

        foreach (var agent in Agents)
        {
            ApplyContamination(agent.Value);
        }
    }

    public State(State parent, List<Tuple<Types.RouteAction, List<string>>> actions)
    {
        Parent = parent;
        Contamination = (byte[,]) Parent.Contamination.Clone();
        Agents = Parent.Agents;
        Commands = Parent.Commands;
        G = Parent.G + 1;

        



        //// APPLY TEMPLATES AND EXPAND STATES
        //foreach (var item in actions)
        //{
        //    ICommand command = item.Key;
        //    Types.RouteAction action = item.Value;

        //    switch (command)
        //    {
        //        case Move moveCommand:
        //            Agent agent = Agents[moveCommand.DropletName];
        //            agent.PositionX += action.Droplet1XDelta;
        //            agent.PositionY += action.Droplet1YDelta;
        //            ApplyContamination(agent);
        //            break;

        //        case Merge mergeCommand:
        //            //boardActions.AddRange(HandleMergeCommand(mergeCommand));
        //            break;
        //        case SplitByRatio splitByRatioCommand:
        //            //boardActions.AddRange(HandleSplitByRatioCommand(splitByRatioCommand));
        //            break;
        //        case SplitByVolume splitByVolumeCommand:
        //            //boardActions.AddRange(HandleSplitByVolumeCommand(splitByVolumeCommand));
        //            break;
        //        default:
        //            Console.WriteLine("Unknown command");
        //            break;
        //    }

        //}
    }

    private int CalculateHeuristic()
    {
        // Do something
        return H;
    }

    private List<State> GetExpandedStates()
    {

        List<List<Tuple<Types.RouteAction, List<string>>>> applicableActions;

        foreach (var agent in Agents)
        {
            Tuple<Types.RouteAction, List<string>> moveUp = new Tuple<Types.RouteAction, List<string>>(Types.RouteAction.MoveUp, new List<string>(){ agent.Key });


            ApplicableFunctions.IsApplicable(moveUp, Agents, Contamination);
        }

        return null;
    }

    


    public List<BoardAction> ExtractBoardActions()
    {
        //if (BoardActions == null || Parent == null) return new List<BoardAction>();
        
        //List<BoardAction> finalActions = [.. BoardActions];
        //finalActions.AddRange(Parent.ExtractBoardActions());

        //return finalActions;
        return null;
    }

    private void ApplyContamination(Agent agent)
    {
        var x = agent.PositionX;
        var y = agent.PositionY;

        int rowCount = Contamination.GetLength(0);
        int colCount = Contamination.GetLength(1);

        void ApplyIfInBounds(int xPos, int yPos)
        {
            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                Contamination[xPos, yPos] = (byte)(Contamination[xPos, yPos] == 0 ? agent.SubstanceId : 255);
            }
        }

        ApplyIfInBounds(x, y);
        ApplyIfInBounds(x + 1, y);
        ApplyIfInBounds(x - 1, y);
        ApplyIfInBounds(x, y + 1);
        ApplyIfInBounds(x, y - 1);

        ApplyIfInBounds(x + 1, y + 1);
        ApplyIfInBounds(x + 1, y - 1);
        ApplyIfInBounds(x - 1, y + 1);
        ApplyIfInBounds(x - 1, y - 1);
    }

}

