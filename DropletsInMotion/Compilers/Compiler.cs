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

        public Compiler(List<ICommand> commands, Dictionary<string, Droplet> droplets, CommunicationEngine communicationEngine, string platformPath)
        {
            CommunicationEngine = communicationEngine;

            PlatformService = new PlatformService(platformPath);

            Board = PlatformService.Board;

            Console.WriteLine(Board[0][1]);
            TemplateHandler = new TemplateHandler(Board);

            DependencyGraph = new DependencyGraph(commands);

            //DependencyGraph.GenerateDotFile();

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
                //_router.Route(Droplets, commandsToExecute, time);
                List<ICommand> movesToExecute = new List<ICommand>();

                foreach (ICommand command in commandsToExecute)
                {
                    if (command is Move)
                    {
                        movesToExecute.Add(command);
                    }
                }

                foreach (ICommand command in commandsToExecute)
                {
                    switch (command)
                    {
                        case Merge mergeCommand:
                            if (InPositionToMerge(mergeCommand, movesToExecute))
                            {
                                boardActions.AddRange(_router.Merge(Droplets, mergeCommand, time));
                            }
                            break;
                        case SplitByRatio splitByRatioCommand:
                            boardActions.AddRange(HandleSplitByRatioCommand(splitByRatioCommand));
                            break;
                        case SplitByVolume splitByVolumeCommand:
                            boardActions.AddRange(HandleSplitByVolumeCommand(splitByVolumeCommand));
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }





                boardActions.AddRange(_router.Route(Droplets, movesToExecute, time));
                boardActions = boardActions.OrderBy(b => b.Time).ToList();
                time = boardActions.Any() ? boardActions.Last().Time : time;
                

                //Console.WriteLine("\nPRINTING ACTIONS\n");
                //foreach (var action in boardActions)
                //{
                //    Console.WriteLine(action.ToString());
                //}


                DependencyGraph.updateExecutedNodes(executableNodes, Droplets);

                //foreach (DependencyNode node in executableNodes)
                //{
                //    DependencyGraph.MarkNodeAsExecuted(node.NodeId);
                //}
                //i += 1;

                await CommunicationEngine.SendActions(boardActions);
                boardActions.Clear();

            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());


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
                Console.WriteLine("Droplets are in position to merge");
                return true;
            }

            // If they are not in position, generate move commands to bring them to the target position
            int mergePositionX = mergeCommand.PositionX;
            int mergePositionY = mergeCommand.PositionY;

            // Move inputDroplet1 to be next to the merge position
            if (Math.Abs(inputDroplet1.PositionX - mergePositionX) > 1 || Math.Abs(inputDroplet1.PositionY - mergePositionY) > 1)
            {
                var moveCommand = new Move(inputDroplet1.DropletName, mergePositionX - 1, mergePositionY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move command added for droplet 1: {moveCommand}");
            }

            // Move inputDroplet2 to be next to the merge position
            if (Math.Abs(inputDroplet2.PositionX - mergePositionX) > 1 || Math.Abs(inputDroplet2.PositionY - mergePositionY) > 1)
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
