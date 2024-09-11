using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Controllers;
using DropletsInMotion.Domain;

namespace DropletsInMotion.Routers;

/*
 * Assumption:
 * The electrode under a droplet will always be on, until the droplet is wasted or outputted
 * The movement is only as good as the design of the templates
 */

/*
 * In: Board, Droplets, List<ICommand>
 * Out: List<BoardAction>
 *
 * Droplets is the state
 */


public class Router
{
    private Electrode[][] Board { get; set; }
    private List<Droplet> Droplets { get; set; }
    private List<ICommand> Commands { get; set; }
    private double Time { get; set; }


    private readonly MoveHandler _moveHandler;
    private readonly TemplateHandler _templateHandler;


    public Router(Electrode[][] board)
    {
        Board = board;
        _templateHandler = new TemplateHandler(Board);
        _moveHandler = new MoveHandler(_templateHandler);
    }

    public List<BoardAction> Route(List<Droplet> droplets, List<ICommand> commands, double time)
    {
        Droplets = droplets;
        Commands = commands;
        Time = time;
        List<BoardAction> boardActions = new List<BoardAction>();


        Console.WriteLine("\n\n\nRunning router.....\n");
        
        
        foreach (var command in commands)
        {
            switch (command)
            {
                case Move moveCommand:
                    boardActions.AddRange(HandleMoveCommand(moveCommand));
                    break;
                case Merge mergeCommand:
                    HandleMergeCommand(mergeCommand);
                    break;
                case Mix mixCommand:
                    HandleMixCommand(mixCommand);
                    break;
                case SplitByRatio splitByRatioCommand:
                    boardActions.AddRange(HandleSplitByRatioCommand(splitByRatioCommand));
                    break;
                case SplitByVolume splitByVolumeCommand:
                    HandleSplitByVolumeCommand(splitByVolumeCommand);
                    break;
                case Store storeCommand:
                    HandleStoreCommand(storeCommand);
                    break;
                case Dispense dispenseCommand:
                    HandleDispenseCommand(dispenseCommand);
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }

        return boardActions;
    }


    private List<BoardAction> HandleMoveCommand(Move moveCommand)
    {
        Console.WriteLine($"Moving droplet to ({moveCommand.PositionX}, {moveCommand.PositionY})");

        Droplet droplet = Droplets.Find(d => d.Name == moveCommand.DropletName)
                            ?? throw new InvalidOperationException($"No droplet found with name {moveCommand.DropletName}.");
        if (droplet == null)
        {
            throw new InvalidOperationException($"Droplet with name {moveCommand.DropletName} not found.");
        }

        double time = Time;

        return _moveHandler.MoveDroplet(droplet, moveCommand.PositionX, moveCommand.PositionY, ref time);
    }

    private void HandleMergeCommand(Merge mergeCommand)
    {
        // Add logic for processing the Merge command
        Console.WriteLine($"Merging droplets with IDs: {mergeCommand.InputName1}, {mergeCommand.InputName2}");
    }

    private List<BoardAction> HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand)
    {
        // Add logic for processing the SplitByRatio command
        Console.WriteLine($"Splitting droplet with ratio {splitByRatioCommand.Ratio}");


        Droplet inputDroplet = Droplets.Find(d => d.Name == splitByRatioCommand.InputName)
                               ?? throw new InvalidOperationException($"No droplet found with name {splitByRatioCommand.InputName}.");
        

        // Create the new droplets
        Droplet outputDroplet1 = new Droplet(splitByRatioCommand.OutputName1, inputDroplet.PositionX - 1,
            inputDroplet.PositionY, inputDroplet.Volume * (1 - splitByRatioCommand.Ratio));

        Droplet outputDroplet2 = new Droplet(splitByRatioCommand.OutputName2, inputDroplet.PositionX + 1,
            inputDroplet.PositionY, inputDroplet.Volume * splitByRatioCommand.Ratio);

        Droplets.Remove(inputDroplet);
        Droplets.Add(outputDroplet1);
        Droplets.Add(outputDroplet2);

        List<BoardAction> splitActions = new List<BoardAction>();

        splitActions.AddRange(_templateHandler.ApplyTemplate("splitHorizontal", inputDroplet, Time));

        double time1 = splitActions.Last().Time;
        double time2 = splitActions.Last().Time;
        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet1, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1, ref time1));
        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet2, splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2, ref time2));

        return splitActions;
    }

    private void HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand)
    {
        // Add logic for processing the SplitByVolume command
        Console.WriteLine($"Splitting droplet by volume {splitByVolumeCommand.Volume}");
    }

    private void HandleMixCommand(Mix mixCommand)
    {
        // Add logic for processing the Mix command
        Console.WriteLine("Mixing droplets...");
    }

    private void HandleStoreCommand(Store storeCommand)
    {
        // Add logic for processing the Store command
        Console.WriteLine($"Storing droplet in {storeCommand.PositionX}, {storeCommand.PositionY} for {storeCommand.Time} seconds...");
    }

    private void HandleWaitCommand(Wait waitCommand)
    {
        // Add logic for processing the Wait command
        Console.WriteLine($"Waiting for {waitCommand.Time} seconds...");
    }

    private void HandleWaitForUserInputCommand(WaitForUserInput waitForUserInputCommand)
    {
        // Add logic for processing the WaitForUserInput command
        Console.WriteLine("Waiting for user input...");
    }

    private void HandleDispenseCommand(Dispense dispenseCommand)
    {
        // Add logic for processing the Dispense command
        Console.WriteLine($"Dispensing droplet at {dispenseCommand.DropletName}");
    }

}

