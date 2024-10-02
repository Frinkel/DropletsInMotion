//using DropletsInMotion.Application.ExecutionEngine.Models;
//using DropletsInMotion.Infrastructure.Models.Commands;
//using DropletsInMotion.Infrastructure.Models.Domain;

//namespace DropletsInMotion.Application.Services.Routers;

///*
// * Assumption:
// * The electrode under a droplet will always be on, until the droplet is wasted or outputted
// * The movement is only as good as the design of the Templates
// */

///*
// * In: Board, Droplets, List<IDropletCommand>
// * Out: List<BoardAction>
// *
// * Droplets is the state
// */


//public class SimpleRouter
//{
//    private Electrode[][] Board { get; set; }
//    public Dictionary<string, Droplet> Droplets { get; set; }
//    private List<IDropletCommand> Commands { get; set; }
//    private double Time { get; set; }


//    private readonly MoveHandler _moveHandler;
//    private readonly TemplateService _templateHandler;


//    public SimpleRouter(Electrode[][] board)
//    {
//        Board = board;
//        _templateHandler = new TemplateService(Board);
//        _moveHandler = new MoveHandler(_templateHandler);
//    }

//    public List<BoardAction> Route(Dictionary<string, Droplet> droplets, List<IDropletCommand> commands, double time)
//    {
//        Droplets = droplets;
//        Commands = commands;
//        Time = time;
//        List<BoardAction> boardActions = new List<BoardAction>();


//        //Console.WriteLine("\n\n\nRunning router.....\n");


//        foreach (var dropletCommand in Commands)
//        {
//            switch (dropletCommand)
//            {
//                case Move moveCommand:
//                    boardActions.AddRange(HandleMoveCommand(moveCommand));
//                    break;
//                case Merge mergeCommand:
//                    boardActions.AddRange(HandleMergeCommand(mergeCommand));
//                    break;
//                case SplitByRatio splitByRatioCommand:
//                    boardActions.AddRange(HandleSplitByRatioCommand(splitByRatioCommand));
//                    break;
//                case SplitByVolume splitByVolumeCommand:
//                    boardActions.AddRange(HandleSplitByVolumeCommand(splitByVolumeCommand));
//                    break;
//                case Mix mixCommand:
//                    boardActions.AddRange(HandleMixCommand(mixCommand));
//                    break;
//                case Dispense dispenseCommand:
//                    HandleDispenseCommand(dispenseCommand);
//                    break;
//                default:
//                    Console.WriteLine("Unknown dropletCommand");
//                    break;
//            }
//        }

//        return boardActions;
//    }


//    private List<BoardAction> HandleMoveCommand(Move moveCommand)
//    {
//        //Console.WriteLine($"Moving droplet to ({moveCommand.PositionX}, {moveCommand.PositionY})");

//        Droplet droplet = Droplets[moveCommand.DropletName]
//                            ?? throw new InvalidOperationException($"No droplet found with name {moveCommand.DropletName}.");
//        if (droplet == null)
//        {
//            throw new InvalidOperationException($"Droplet with name {moveCommand.DropletName} not found.");
//        }

//        double time = Time;

//        return _moveHandler.MoveDroplet(droplet, moveCommand.PositionX, moveCommand.PositionY, ref time);
//    }

//    private List<BoardAction> HandleMergeCommand(Merge mergeCommand)
//    {
//        // Add logic for processing the Merge dropletCommand
//        //Console.WriteLine($"Merging droplets with IDs: {mergeCommand.InputName1}, {mergeCommand.InputName2}");

//        Droplet inputDroplet1 = Droplets[mergeCommand.InputName1]
//                                ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");

//        Droplet inputDroplet2 = Droplets[mergeCommand.InputName2]
//                                ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");

//        List<BoardAction> mergeActions = new List<BoardAction>();

//        double time1 = Time;
//        double time2 = Time;
//        mergeActions.AddRange(_moveHandler.MoveDroplet(inputDroplet1, mergeCommand.PositionX - 1, mergeCommand.PositionY, ref time1));
//        mergeActions.AddRange(_moveHandler.MoveDroplet(inputDroplet2, mergeCommand.PositionX + 1, mergeCommand.PositionY, ref time2));

//        Droplet outputDroplet = new Droplet(mergeCommand.OutputName, mergeCommand.PositionX, mergeCommand.PositionY,
//            inputDroplet1.Volume + inputDroplet2.Volume);

//        Droplets.Remove(inputDroplet1.DropletName);
//        Droplets.Remove(inputDroplet2.DropletName);
//        Droplets[outputDroplet.DropletName] = outputDroplet;

//        mergeActions = mergeActions.OrderBy(b => b.Time).ToList();

//        double time = mergeActions.Any() ? mergeActions.Last().Time : Time;

//        mergeActions.AddRange(_templateHandler.ApplyTemplate("mergeHorizontal", outputDroplet, time));

//        return mergeActions;
//    }

//    private List<BoardAction> HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand)
//    {
//        // Add logic for processing the SplitByRatio dropletCommand
//        //Console.WriteLine($"Splitting droplet with ratio {splitByRatioCommand.Ratio}");


//        Droplet inputDroplet = Droplets[splitByRatioCommand.InputName]
//                               ?? throw new InvalidOperationException($"No droplet found with name {splitByRatioCommand.InputName}.");


//        // Create the new droplets
//        Droplet outputDroplet1 = new Droplet(splitByRatioCommand.OutputName1, inputDroplet.PositionX - 1,
//            inputDroplet.PositionY, inputDroplet.Volume * (1 - splitByRatioCommand.Ratio));

//        Droplet outputDroplet2 = new Droplet(splitByRatioCommand.OutputName2, inputDroplet.PositionX + 1,
//            inputDroplet.PositionY, inputDroplet.Volume * splitByRatioCommand.Ratio);

//        Droplets.Remove(inputDroplet.DropletName);
//        Droplets[outputDroplet1.DropletName] = outputDroplet1;
//        Droplets[outputDroplet2.DropletName] = outputDroplet2;

//        List<BoardAction> splitActions = new List<BoardAction>();

//        splitActions.AddRange(_templateHandler.ApplyTemplate("splitHorizontal", inputDroplet, Time));

//        double time1 = splitActions.Any() ? splitActions.Last().Time : Time;
//        double time2 = splitActions.Any() ? splitActions.Last().Time : Time;
//        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet1, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1, ref time1));
//        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet2, splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2, ref time2));

//        return splitActions;
//    }

//    private List<BoardAction> HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand)
//    {
//        // Add logic for processing the SplitByVolume dropletCommand
//        //Console.WriteLine($"Splitting droplet by volume {splitByVolumeCommand.Volume}");

//        Droplet inputDroplet = Droplets[splitByVolumeCommand.InputName]
//                               ?? throw new InvalidOperationException($"No droplet found with name {splitByVolumeCommand.InputName}.");


//        // Create the new droplets
//        Droplet outputDroplet1 = new Droplet(splitByVolumeCommand.OutputName1, inputDroplet.PositionX - 1,
//            inputDroplet.PositionY, inputDroplet.Volume - splitByVolumeCommand.Volume);

//        Droplet outputDroplet2 = new Droplet(splitByVolumeCommand.OutputName2, inputDroplet.PositionX + 1,
//            inputDroplet.PositionY, splitByVolumeCommand.Volume);

//        Droplets.Remove(inputDroplet.DropletName);
//        Droplets[outputDroplet1.DropletName] = outputDroplet1;
//        Droplets[outputDroplet2.DropletName] = outputDroplet2;

//        List<BoardAction> splitActions = new List<BoardAction>();

//        splitActions.AddRange(_templateHandler.ApplyTemplate("splitHorizontal", inputDroplet, Time));

//        double time1 = splitActions.Any() ? splitActions.Last().Time : Time;
//        double time2 = splitActions.Any() ? splitActions.Last().Time : Time;
//        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet1, splitByVolumeCommand.PositionX1, splitByVolumeCommand.PositionY1, ref time1));
//        splitActions.AddRange(_moveHandler.MoveDroplet(outputDroplet2, splitByVolumeCommand.PositionX2, splitByVolumeCommand.PositionY2, ref time2));

//        return splitActions;
//    }

//    private List<BoardAction> HandleMixCommand(Mix mixCommand)
//    {
//        // Add logic for processing the Mix dropletCommand
//        //Console.WriteLine("Mixing droplets...");

//        Droplet inputDroplet = Droplets[mixCommand.DropletName]
//                               ?? throw new InvalidOperationException($"No droplet found with name {mixCommand.DropletName}.");

//        List<BoardAction> mixActions = new List<BoardAction>();

//        double time = Time;
//        mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, mixCommand.PositionX, mixCommand.PositionY, ref time));

//        double time1 = mixActions.Any() ? mixActions.Last().Time : Time;

//        for (int i = 0; i < mixCommand.RepeatTimes; i++)
//        {
//            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX + mixCommand.Width, inputDroplet.PositionY, ref time1));
//            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY + mixCommand.Height, ref time1));
//            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX - mixCommand.Width, inputDroplet.PositionY, ref time1));
//            mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY - mixCommand.Height, ref time1));
//        }

//        return mixActions;
//    }

//    private void HandleDispenseCommand(Dispense dispenseCommand)
//    {
//        throw new NotImplementedException();
//        // Add logic for processing the Dispense dropletCommand
//        Console.WriteLine($"Dispensing droplet at {dispenseCommand.DropletName}");
//    }

//}

