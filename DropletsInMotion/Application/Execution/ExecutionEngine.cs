using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Communication;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Presentation.Services;

namespace DropletsInMotion.Application.Execution
{
    public class ExecutionEngine : IExecutionEngine
    {
        //public CommunicationEngine CommunicationEngine;

        public Electrode[][] Board { get; set; }

        public Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();

        public double Time { get; set; }
        private byte[,] ContaminationMap { get; set; }

        private PlatformService PlatformService;

        private DependencyGraph DependencyGraph;

        //private SimpleRouter _simpleRouter;
        private readonly IRouterService _router;
        private readonly IContaminationService _contaminationService;
        private readonly ISchedulerService _scheduler;
        private readonly IStoreService _storeManager;
        private readonly ICommandLifetimeService _commandManager;
        private readonly ITimeService _timeService;
        private readonly IActionService _actionService;
        private readonly ITemplateService _templateService;
        private readonly IDependencyService _dependencyService;
        private readonly ICommunicationService _communicationService;


        public ExecutionEngine(IContaminationService contaminationService, ISchedulerService schedulerService, 
                                IStoreService storeService, ICommandLifetimeService commandLifetimeService, ITimeService timeService, 
                                IActionService actionService, IRouterService routerService, IDependencyService dependencyService, 
                                ITemplateService templateService, ICommunicationService communicationService)
        {
            _contaminationService = contaminationService;
            _scheduler = schedulerService;
            _storeManager = storeService;
            _commandManager = commandLifetimeService;
            _timeService = timeService;
            _actionService = actionService;
            _router = routerService;
            _templateService = templateService;
            _dependencyService = dependencyService;
            _communicationService = communicationService;
        }

        public async Task Execute(List<ICommand> commands, Dictionary<string, Droplet> droplets, string platformPath)
        {
            PlatformService = new PlatformService(platformPath);

            Board = PlatformService.Board;

            Console.WriteLine(Board[0][1]);
            _templateService.Initialize(Board);

            DependencyGraph = new DependencyGraph(commands);

            DependencyGraph.GenerateDotFile();

            //Droplets = droplets;
            foreach (var dropleKvp in droplets)
            {
                var droplet = dropleKvp.Value;
                Agents.Add(dropleKvp.Key, new Agent(droplet.DropletName, droplet.PositionX, droplet.PositionY, droplet.Volume));
            }

            ContaminationMap = new byte[Board.Length, Board[0].Length];



            //_simpleRouter = new SimpleRouter(Board);
            _router.Initialize(Board, Agents);
            //_router = new RouterService(Board, Droplets, _contamination);



            var watch = System.Diagnostics.Stopwatch.StartNew();


            List<BoardAction> boardActions = new List<BoardAction>();
            int i = 0;


            while (DependencyGraph.GetExecutableNodes().Count > 0)
            {

                List<DependencyNode> executableNodes = DependencyGraph.GetExecutableNodes();
                List<ICommand> commandsToExecute = executableNodes.ConvertAll(node => node.Command);
                //print the commands

                Console.WriteLine($"Commands to execute iteration {i}:");
                i++;
                foreach (ICommand command in commandsToExecute)
                {
                    Console.WriteLine(command);
                }

                List<ICommand> movesToExecute = new List<ICommand>();

                double? executionTime = Time;
                foreach (ICommand command in commandsToExecute)
                {
                    switch (command)
                    {
                        case Move moveCommand:
                            movesToExecute.Add(moveCommand);
                            break;
                        case Merge mergeCommand:
                            //TODO: create scheduler here
                            await HandleMergeCommand(mergeCommand, movesToExecute, boardActions, executionTime);
                            break;
                        case SplitByRatio splitByRatioCommand:
                            SplitByVolume splitByVolumeCommand2 = new SplitByVolume(splitByRatioCommand.InputName, splitByRatioCommand.OutputName1,
                                splitByRatioCommand.OutputName2, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1,
                                splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2,
                                Agents.ContainsKey(splitByRatioCommand.InputName) ? Agents[splitByRatioCommand.InputName].Volume * splitByRatioCommand.Ratio : 1);
                            if (!HasSplit(splitByVolumeCommand2, movesToExecute))
                            {
                                _commandManager.StoreCommand(splitByVolumeCommand2);
                                boardActions.AddRange(_actionService.SplitByVolume(Agents, splitByVolumeCommand2, ContaminationMap, Time, 1));
                                executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time : Time;

                            }
                            break;
                        case SplitByVolume splitByVolumeCommand:

                            //TODO: create scheduler here

                            if (!HasSplit(splitByVolumeCommand, movesToExecute))
                            {
                                _commandManager.StoreCommand(command);
                                boardActions.AddRange(_actionService.SplitByVolume(Agents, splitByVolumeCommand, ContaminationMap, Time, 1));
                                executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time : Time;
                            }
                            break;
                        case Store storeCommand:
                            if (_actionService.InPositionToStore(storeCommand, Agents, movesToExecute))
                            {
                                _storeManager.StoreDroplet(storeCommand, Time);
                            }
                            break;
                        case Mix mixCommand:
                            await HandleMixCommand(mixCommand, movesToExecute);
                            break;
                        case WaitForUserInput waitForUserInputCommand:
                            Console.WriteLine("");
                            Console.WriteLine("Press enter to continue");
                            Console.WriteLine("");
                            // MAYBE ADD STOPWATCH TO EXTEND TIME WITH GIVEN AMOUNT?
                            Console.ReadLine();
                            break;
                        case Wait waitCommand:
                            executionTime = waitCommand.Time + Time;
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }

                double? boundTime = _timeService.CalculateBoundTime(Time, executionTime);
                if (movesToExecute.Count > 0)
                {
                    boardActions.AddRange(_router.Route(Agents, movesToExecute, ContaminationMap, Time, boundTime));
                    boardActions = boardActions.OrderBy(b => b.Time).ToList();
                    Time = boardActions.Any() ? boardActions.Last().Time : Time;

                }
                else
                {
                    Time = boundTime != null ? (double)boundTime : Time;
                }



                _dependencyService.updateExecutedNodes(executableNodes, Agents, Time);

                if (boardActions.Count > 0)
                {
                    await _communicationService.SendActions(boardActions);
                }
                Console.WriteLine($"Compiler time {Time}");
                boardActions.Clear();
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

        }

        private async Task HandleMixCommand(Mix mixCommand, List<ICommand> movesToExecute)
        {
            if (_actionService.InPositionToMix(mixCommand, Agents, movesToExecute))
            {
                List<BoardAction> mixActions = new List<BoardAction>();
                mixActions.AddRange(_actionService.Mix(Agents, mixCommand, ContaminationMap, Time));
                _storeManager.StoreDropletWithNameAndTime(mixCommand.DropletName, Time + mixActions.Last().Time);
                
                await _communicationService.SendActions(mixActions);
                
            }
        }



        

        // TODO: Maybe into movehandler
        private bool HasSplit(SplitByVolume splitCommand, List<ICommand> movesToExecute)
        {

            if (_commandManager.CanExecuteCommand(splitCommand))
            {

                if (Agents.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
                {
                    throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName1} already exists.");
                }

                if (Agents.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
                {
                    throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName2} already exists.");
                }

                if (splitCommand.OutputName2 == splitCommand.OutputName1)
                {
                    throw new InvalidOperationException($"Droplet with the same names can not be split.");
                }

                //TODO change scheduler to take agents instead of droplets
                var droplets = new Dictionary<string, Droplet>();
                foreach (var agentLKvp in Agents)
                {
                    var agent = agentLKvp.Value;
                    droplets.Add(agent.DropletName, new Droplet(agent.DropletName, agent.PositionX, agent.PositionY, agent.Volume));
                }

                var splitPositions = _scheduler.ScheduleCommand(splitCommand, droplets, Agents,
                    ContaminationMap);

                // TODO: ALEX MAKE THIS MORE READABLE
                int splitPositionX = (splitPositions.Value.Item1.optimalX + splitPositions.Value.Item2.optimalX) / 2;
                int splitPositionY = (splitPositions.Value.Item1.optimalY + splitPositions.Value.Item2.optimalY) / 2;

                Droplet splitDroplet = Agents[splitCommand.InputName];

                if (splitDroplet.PositionX == splitPositionX && splitDroplet.PositionY == splitPositionY)
                {
                    return false;
                }

                movesToExecute.Add(new Move(splitCommand.InputName, splitPositionX, splitPositionY));

                return true;
            }

            if (Agents.TryGetValue(splitCommand.OutputName1, out Agent outputDroplet1))
            {
                if (outputDroplet1.PositionX != splitCommand.PositionX1 || outputDroplet1.PositionY != splitCommand.PositionY1)
                {
                    movesToExecute.Add(new Move(splitCommand.OutputName1, splitCommand.PositionX1, splitCommand.PositionY1));
                }
            }
            if (Agents.TryGetValue(splitCommand.OutputName2, out Agent outputDroplet2))
            {
                if (outputDroplet2.PositionX != splitCommand.PositionX2 || outputDroplet2.PositionY != splitCommand.PositionY2)
                {
                    movesToExecute.Add(new Move(splitCommand.OutputName2, splitCommand.PositionX2, splitCommand.PositionY2));
                }
            }

            return true;
        }

        // TODO: Maybe into movehandler

        private async Task HandleMergeCommand(Merge mergeCommand, List<ICommand> movesToExecute, List<BoardAction> boardActions, double? executionTime)
        {

                if (InPositionToMerge(mergeCommand, movesToExecute))
                {
                    _commandManager.StoreCommand(mergeCommand);
                    boardActions.AddRange(_actionService.Merge(Agents, mergeCommand, ContaminationMap, Time));
                    executionTime = boardActions.Any() && boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time;
                }
            


        }

        private bool InPositionToMerge(Merge mergeCommand, List<ICommand> movesToExecute)
        {




            if (_commandManager.CanExecuteCommand(mergeCommand))
            {
                if (!Agents.TryGetValue(mergeCommand.InputName1, out var inputDroplet1))
                {
                    throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");
                }

                if (!Agents.TryGetValue(mergeCommand.InputName2, out var inputDroplet2))
                {
                    throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");
                }

                var droplets = new Dictionary<string, Droplet>();
                foreach (var agentLKvp in Agents)
                {
                    var  agent = agentLKvp.Value;
                    droplets.Add(agent.DropletName, new Droplet(agent.DropletName, agent.PositionX, agent.PositionY, agent.Volume));
                }

                var mergePositions = _scheduler.ScheduleCommand(mergeCommand, droplets, Agents,
                    ContaminationMap);
                // Check if the droplets are in position for the merge (1 space apart horizontally or vertically)


                bool areInPosition = inputDroplet1.PositionX == mergePositions.Value.Item1.optimalX &&
                                      inputDroplet1.PositionY == mergePositions.Value.Item1.optimalY &&
                                      inputDroplet2.PositionX == mergePositions.Value.Item2.optimalX &&
                                      inputDroplet2.PositionY == mergePositions.Value.Item2.optimalY;

                // If the droplets are already in position, return true
                if (areInPosition)
                {
                    return true;
                }

                // Move inputDroplet1 to be next to the merge position
                if (inputDroplet1.PositionX != mergePositions.Value.Item1.optimalX || inputDroplet1.PositionY != mergePositions.Value.Item1.optimalY)
                {
                    var moveCommand = new Move(inputDroplet1.DropletName, mergePositions.Value.Item1.optimalX, mergePositions.Value.Item1.optimalY);
                    movesToExecute.Add(moveCommand);
                    Console.WriteLine($"Move command added for droplet 1: {moveCommand}");
                }

                // Move inputDroplet2 to be next to the merge position
                if (inputDroplet2.PositionX != mergePositions.Value.Item2.optimalX || inputDroplet2.PositionY != mergePositions.Value.Item2.optimalY)
                {
                    var moveCommand = new Move(inputDroplet2.DropletName, mergePositions.Value.Item2.optimalX, mergePositions.Value.Item2.optimalY);
                    movesToExecute.Add(moveCommand);
                    Console.WriteLine($"Move command added for droplet 2: {moveCommand}");

                }

                // Return false because the droplets are not yet in position and need to move
                Console.WriteLine("Droplets are NOT in position to merge");
                return false;

            }
            else
            {
                if (!Agents.TryGetValue(mergeCommand.OutputName, out var outDroplet))
                {
                    throw new InvalidOperationException($"No droplet found with name {mergeCommand.OutputName}.");
                }

                var moveCommand = new Move(mergeCommand.OutputName, mergeCommand.PositionX, mergeCommand.PositionY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move command added for droplet 2: {moveCommand}");


                return false;
            }

        }

    }
}
