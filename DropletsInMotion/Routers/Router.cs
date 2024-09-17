using DropletsInMotion.Controllers;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

    public void Route(Dictionary<string, Droplet> droplets, List<ICommand> commands, double time)
    {

        // TODO: Should happen by triggering a function?
        // Create all new agents and add their contamination
        //foreach (var dropletKvp in droplets)
        //{
        //    var droplet = dropletKvp.Value;

        //    if (!Agents.ContainsKey(dropletKvp.Key))
        //    {
        //        Agent agent = new Agent(droplet.DropletName, droplet.PositionX, droplet.PositionY, droplet.Volume);
        //        Agents.Add(dropletKvp.Key, agent);
        //        ContaminationMap = ApplicableFunctions.ApplyContamination(agent, ContaminationMap);
        //    }
        //}


        List<string> routableAgents = new List<string>();

        foreach (var command in commands)
        {
            routableAgents.AddRange(command.GetInputDroplets());

            //foreach (var droplet in command.GetInputDroplets())
            //{
            //    Agent agent = new Agent(Agents[droplet].DropletName, Agents[droplet].PositionX, Agents[droplet].PositionY, Agents[droplet].Volume);
            //    routableAgents.Add(droplet);
            //}
        }

        State s0 = new State(routableAgents, Agents, ContaminationMap, commands, _templateHandler);
    }


    



    // TEMP FUNCTIONS
    public void PrintContaminationState()
    {
        // Determine the maximum number of digits for proper alignment
        int maxDigits = 3;

        int rowCount = ContaminationMap.GetLength(0);
        int colCount = ContaminationMap.GetLength(1);

        for (int j = 0; j < colCount; j++)
        {
            for (int i = 0; i < rowCount; i++)
            {
                byte value = ContaminationMap[i, j];

                // Set the color based on the value using a hash function
                SetColorForValue(value);

                // Print each ElectrodeId with a fixed width
                Console.Write(value.ToString().PadLeft(maxDigits) + " ");

                // Reset color after printing
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

    private void SetColorForValue(byte value)
    {
        // Handle 0 as a special case
        if (value == 0)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else
        {
            // Get a unique color for each value using a hash function
            var rgbColor = GetRGBFromHash(value);

            // Convert RGB to closest ConsoleColor
            var consoleColor = GetConsoleColorFromRGB(rgbColor.R, rgbColor.G, rgbColor.B);
            Console.BackgroundColor = consoleColor;
        }
    }

    // Method to generate an RGB color from a hash of the value
    private (byte R, byte G, byte B) GetRGBFromHash(byte value)
    {
        // Hash the value (using a simple hash function for demonstration)
        int hash = (int)(value * 2654435761); // A simple, fast hash function

        // Extract RGB components from the hash
        byte r = (byte)((hash >> 16) & 0xFF);
        byte g = (byte)((hash >> 8) & 0xFF);
        byte b = (byte)(hash & 0xFF);

        return (r, g, b);
    }

    // Convert RGB to a ConsoleColor (approximation due to limited colors in Console)
    private ConsoleColor GetConsoleColorFromRGB(byte r, byte g, byte b)
    {
        // Use a simple mapping of RGB to the nearest ConsoleColor
        if (r > 128)
        {
            if (g > 128)
            {
                if (b > 128) return ConsoleColor.DarkRed;
                else return ConsoleColor.Yellow;
            }
            else
            {
                if (b > 128) return ConsoleColor.Magenta;
                else return ConsoleColor.Red;
            }
        }
        else
        {
            if (g > 128)
            {
                if (b > 128) return ConsoleColor.Cyan;
                else return ConsoleColor.Green;
            }
            else
            {
                if (b > 128) return ConsoleColor.Blue;
                else return ConsoleColor.DarkCyan;
            }
        }
    }
}

