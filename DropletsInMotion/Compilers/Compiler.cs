using System.ComponentModel;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using DropletsInMotion.Domain;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Compilers.Services;
using DropletsInMotion.Controllers;
using DropletsInMotion.Controllers.ConsoleController;
using DropletsInMotion.Routers;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Compilers
{
    public class 
        Compiler
    {
        public CommunicationEngine CommunicationEngine;

        public Electrode[][] Board { get; set; }

        public Dictionary<string, Droplet> Droplets { get; set; } = new Dictionary<string, Droplet>();

        public double time = 0;

        private TemplateHandler TemplateHandler;

        private PlatformService PlatformService;

        private DependencyGraph DependencyGraph;

        private readonly SimpleRouter _simpleRouter;
        private Router _router;

        private StoreManager StoreManager = new StoreManager();

        public Compiler(List<ICommand> commands, Dictionary<string, Droplet> droplets, CommunicationEngine communicationEngine, string platformPath)
        {
            CommunicationEngine = communicationEngine;

            PlatformService = new PlatformService(platformPath);

            Board = PlatformService.Board;

            Console.WriteLine(Board[0][1]);
            TemplateHandler = new TemplateHandler(Board);

            DependencyGraph = new DependencyGraph(commands);

            DependencyGraph.GenerateDotFile();

            Droplets = droplets;

            _simpleRouter = new SimpleRouter(Board);
            _router = new Router(Board, Droplets);

        }

        public async Task Compile()
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();


            List<BoardAction> boardActions = new List<BoardAction>();
            int i = 0;


            while (DependencyGraph.GetExecutableNodes().Count > 0)
            {

                List<DependencyNode> executableNodes = DependencyGraph.GetExecutableNodes();
                List<ICommand> commandsToExecute = executableNodes.ConvertAll(node => node.Command);
                //print the commands

                Console.WriteLine($"Commands to execute iteration {i}:");
                foreach (ICommand command in commandsToExecute)
                {
                    Console.WriteLine(command);
                }

                List<ICommand> movesToExecute = new List<ICommand>();

                double? executionTime = time;
                foreach (ICommand command in commandsToExecute)
                {
                    switch (command)
                    {
                        case Move moveCommand:
                            movesToExecute.Add(moveCommand);
                            break;
                        case Merge mergeCommand:
                            if (InPositionToMerge(mergeCommand, movesToExecute))
                            {
                                boardActions.AddRange(_router.Merge(Droplets, mergeCommand, time));
                                executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : time : time;
                            }
                            break;
                        case SplitByRatio splitByRatioCommand:
                            SplitByVolume splitByVolumeCommand2 = new SplitByVolume(splitByRatioCommand.InputName, splitByRatioCommand.OutputName1, 
                                splitByRatioCommand.OutputName2, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1, 
                                splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2, 
                                Droplets.ContainsKey(splitByRatioCommand.InputName) ? Droplets[splitByRatioCommand.InputName].Volume * splitByRatioCommand.Ratio : 1);
                            if (!hasSplit(splitByVolumeCommand2, movesToExecute))
                            {
                                boardActions.AddRange(_router.SplitByVolume(Droplets, splitByVolumeCommand2, time, 1));
                                executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : time : time;

                            }
                            break;
                        case SplitByVolume splitByVolumeCommand:
                            if (!hasSplit(splitByVolumeCommand, movesToExecute))
                            {
                                boardActions.AddRange(_router.SplitByVolume(Droplets, splitByVolumeCommand, time, 1));
                                executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : time : time;

                            }
                            break;
                        case Store storeCommand:
                            if (InPositionToStore(storeCommand, movesToExecute))
                            {
                                StoreManager.StoreDroplet(storeCommand, time);
                            }
                            break;
                        case Mix mixCommand:
                            if (InPositionToMix(mixCommand, movesToExecute))
                            {
                                List<BoardAction> mixActions = new List<BoardAction>();
                                mixActions.AddRange(_router.Mix(Droplets, mixCommand, time));
                                StoreManager.StoreDropletWithNameAndTime(mixCommand.DropletName, time + mixActions.Last().Time);
                                await CommunicationEngine.SendActions(mixActions);
                            }
                            break;
                        case WaitForUserInput waitForUserInputCommand:
                            Console.WriteLine("");
                            Console.WriteLine("Press enter to continue");
                            Console.WriteLine("");
                            // MAYBE ADD STOPWATCH TO EXTEND TIME WITH GIVEN AMOUNT?
                            Console.ReadLine();
                            break;
                        case Wait waitCommand:
                            executionTime = waitCommand.Time + time;
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }

                double? boundTime = CalculateBoundTime(time, executionTime);
                if (movesToExecute.Count > 0)
                {
                    boardActions.AddRange(_router.Route(Droplets, movesToExecute, time, boundTime));
                    boardActions = boardActions.OrderBy(b => b.Time).ToList();
                    time = boardActions.Any() ? boardActions.Last().Time : time;

                }
                else
                {
                    time = boundTime != null ? (double) boundTime : time;
                }


                
                DependencyGraph.updateExecutedNodes(executableNodes, Droplets, StoreManager, time);

                if (boardActions.Count > 0)
                {
                    await CommunicationEngine.SendActions(boardActions);
                }
                Console.WriteLine($"Compiler time {time}");

                boardActions.Clear();

            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());


        }

        private bool InPositionToMix(Mix mixCommand, List<ICommand> movesToExecute)
        {
            if (StoreManager.ContainsDroplet(mixCommand.DropletName))
            {
                return false;
            }

            if (!Droplets.TryGetValue(mixCommand.DropletName, out var inputDroplet))
            {
                throw new InvalidOperationException($"No droplet found with name {mixCommand.DropletName}.");
            }

            if (inputDroplet.PositionX == mixCommand.PositionX && inputDroplet.PositionY == mixCommand.PositionY)
            {
                return true;
            }
            movesToExecute.Add(new Move(mixCommand.DropletName, mixCommand.PositionX, mixCommand.PositionY));
            return false;
        }

        private double? CalculateBoundTime(double currentTime, double? boundTime)
        {
            if (StoreManager.HasStoredDroplets())
            {
                double nextStoreTime = StoreManager.PeekClosestTime();
                return boundTime > time ? boundTime > nextStoreTime ? nextStoreTime : boundTime : nextStoreTime;
            }

            return boundTime > time ? boundTime : null;
        }

        private bool InPositionToStore(Store storeCommand, List<ICommand> movesToExecute)
        {
            if (!Droplets.TryGetValue(storeCommand.DropletName, out var inputDroplet))
            {
                throw new InvalidOperationException($"No droplet found with name {storeCommand.DropletName}.");
            }

            if (inputDroplet.PositionX == storeCommand.PositionX && inputDroplet.PositionY == storeCommand.PositionY)
            {
                return true;
            }
            movesToExecute.Add(new Move(storeCommand.DropletName, storeCommand.PositionX, storeCommand.PositionY));
            return false;

        }

        private bool hasSplit(SplitByVolume splitCommand, List<ICommand> movesToExecute)
        {
            bool splitOccurred = true;

            if (splitCommand.InputName != splitCommand.OutputName1 &&
                splitCommand.InputName != splitCommand.OutputName1)
            {
                if (Droplets.ContainsKey(splitCommand.InputName)) return false;
            }

            if (splitCommand.InputName == splitCommand.OutputName1)
            {
                if (!Droplets.ContainsKey(splitCommand.OutputName2)) return false;
            }

            if (splitCommand.InputName == splitCommand.OutputName2)
            {
                if (!Droplets.ContainsKey(splitCommand.OutputName1)) return false;
            }

            if (splitOccurred)
            {
                if (Droplets.TryGetValue(splitCommand.OutputName1, out Droplet outputDroplet1))
                {
                    if (outputDroplet1.PositionX != splitCommand.PositionX1 || outputDroplet1.PositionY != splitCommand.PositionY1)
                    {
                        movesToExecute.Add(new Move(splitCommand.OutputName1, splitCommand.PositionX1, splitCommand.PositionY1));
                        splitOccurred = true;
                    }
                }
                if (Droplets.TryGetValue(splitCommand.OutputName2, out Droplet outputDroplet2))
                {
                    if (outputDroplet2.PositionX != splitCommand.PositionX2 || outputDroplet2.PositionY != splitCommand.PositionY2)
                    {
                        movesToExecute.Add(new Move(splitCommand.OutputName2, splitCommand.PositionX2, splitCommand.PositionY2));
                        splitOccurred = true;
                    }
                }
            }
            return splitOccurred;
        }


        private bool InPositionToMerge(Merge mergeCommand, List<ICommand> movesToExecute)
        {
            // Retrieve the two droplets involved in the merge
            if (!Droplets.TryGetValue(mergeCommand.InputName1, out var inputDroplet1))
            {
                throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");
            }

            if (!Droplets.TryGetValue(mergeCommand.InputName2, out var inputDroplet2))
            {
                throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");
            }

            // Check if the droplets are in position for the merge (1 space apart horizontally or vertically)
            bool areInPosition = (Math.Abs(inputDroplet1.PositionX - inputDroplet2.PositionX) == 2 && inputDroplet1.PositionY == inputDroplet2.PositionY) ||
                                 (Math.Abs(inputDroplet1.PositionY - inputDroplet2.PositionY) == 2 && inputDroplet1.PositionX == inputDroplet2.PositionX);

            // If the droplets are already in position, return true
            if (areInPosition)
            {
                return true;
            }

            // If they are not in position, generate move commands to bring them to the target position
            int mergePositionX = mergeCommand.PositionX;
            int mergePositionY = mergeCommand.PositionY;

            // Move inputDroplet1 to be next to the merge position
            if (Math.Abs(inputDroplet1.PositionX - mergePositionX) + Math.Abs(inputDroplet1.PositionY - mergePositionY) > 1)
            {
                var moveCommand = new Move(inputDroplet1.DropletName, mergePositionX - 1, mergePositionY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move command added for droplet 1: {moveCommand}");
            }

            // Move inputDroplet2 to be next to the merge position
            if (Math.Abs(inputDroplet2.PositionX - mergePositionX) + Math.Abs(inputDroplet2.PositionY - mergePositionY) > 1)
            {
                var moveCommand = new Move(inputDroplet2.DropletName, mergePositionX + 1, mergePositionY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move command added for droplet 2: {moveCommand}");

            }

            // Return false because the droplets are not yet in position and need to move
            Console.WriteLine("Droplets are NOT in position to merge");
            return false;
        }


        private IEnumerable<BoardAction> HandleMixCommand(Mix mixCommand)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<BoardAction> HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<BoardAction> HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand)
        {
            throw new NotImplementedException();
        }


    }
}
