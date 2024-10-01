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
                            HandleMergeCommand(mergeCommand, movesToExecute, boardActions, executionTime, Agents);
                            break;
                        case SplitByRatio splitByRatioCommand:
                            HandleSplitByRatioCommand(splitByRatioCommand, movesToExecute, boardActions, executionTime, Agents);
                            break;
                        case SplitByVolume splitByVolumeCommand:
                            HandleSplitByVolumeCommand(splitByVolumeCommand, movesToExecute, boardActions, executionTime, Agents);
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

        private void HandleMergeCommand(Merge mergeCommand, List<ICommand> movesToExecute, List<BoardAction> boardActions, double? executionTime, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(mergeCommand, agents))
            {
                var mergePositions = _scheduler.ScheduleCommand(mergeCommand, agents,
                    ContaminationMap);
                if (_actionService.InPositionToMerge(mergeCommand, movesToExecute, mergePositions.Value, agents))
                {
                    _commandManager.StoreCommand(mergeCommand);
                    boardActions.AddRange(_actionService.Merge(agents, mergeCommand, ContaminationMap, time));
                    executionTime = boardActions.Any() && boardActions.Last().Time > executionTime ? boardActions.Last().Time : time;
                }
            }
            else
            {
                _actionService.MoveMergeDropletToPosition(mergeCommand, movesToExecute, agents);
            }
        }

        private void HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand, List<ICommand> movesToExecute, List<BoardAction> boardActions, double? executionTime, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(splitByVolumeCommand, agents))
            {
                var splitPositions = _scheduler.ScheduleCommand(splitByVolumeCommand, agents,
                    ContaminationMap);
                if (_actionService.InPositionToSplit(splitByVolumeCommand, movesToExecute, splitPositions.Value, agents))
                {
                    _commandManager.StoreCommand(splitByVolumeCommand);
                    boardActions.AddRange(_actionService.SplitByVolume(Agents, splitByVolumeCommand, ContaminationMap, time, splitPositions.Value));
                    executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : time : time;
                }
            }
            else
            {
                _actionService.MoveToSplitToFinalPositions(splitByVolumeCommand, movesToExecute, agents);
            }
        }

        private void HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand, List<ICommand> movesToExecute, List<BoardAction> boardActions, double? executionTime, Dictionary<string, Agent> agents)
        {
            SplitByVolume splitByVolumeCommand2 = new SplitByVolume(splitByRatioCommand.InputName, splitByRatioCommand.OutputName1,
                splitByRatioCommand.OutputName2, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1,
                splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2,
                Agents.ContainsKey(splitByRatioCommand.InputName) ? Agents[splitByRatioCommand.InputName].Volume * splitByRatioCommand.Ratio : 1);
            HandleSplitByVolumeCommand(splitByVolumeCommand2, movesToExecute, boardActions, executionTime, agents);
        }

    }
}
