using System.Reflection.Metadata;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Communication;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Presentation;
using DropletsInMotion.Presentation.Services;

namespace DropletsInMotion.Application.Execution
{
    public class ExecutionEngine : IExecutionEngine
    {
        public Electrode[][] Board { get; set; }

        public Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();

        public Dictionary<string, Double> Variables { get; set; } = new Dictionary<string, Double>();

        public double Time { get; set; }
        private byte[,] ContaminationMap { get; set; }

        private DependencyGraph DependencyGraph;

        private readonly IRouterService _router;
        private readonly IContaminationService _contaminationService;
        private readonly ISchedulerService _schedulerService;
        private readonly IStoreService _storeService;
        private readonly ICommandLifetimeService _commandLifetimeService;
        private readonly ITimeService _timeService;
        private readonly IActionService _actionService;
        //private readonly ITemplateService _templateService;
        private readonly IDependencyService _dependencyService;
        private readonly ITranslator _translator;
        private readonly ICommunicationEngine _communicationEngine;
        private readonly IDeviceRepository _deviceRepository;

        public ExecutionEngine(IContaminationService contaminationService, ISchedulerService schedulerService, 
                                IStoreService storeService, ICommandLifetimeService commandLifetimeService, ITimeService timeService, 
                                IActionService actionService, IRouterService routerService, IDependencyService dependencyService, 
                                ITemplateService templateService, ITranslator translator, ICommunicationEngine communicationEngine, IDeviceRepository deviceRepository)
        {
            _contaminationService = contaminationService;
            _schedulerService = schedulerService;
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
            _timeService = timeService;
            _actionService = actionService;
            _router = routerService;
            //_templateService = templateService;
            _dependencyService = dependencyService;
            _translator = translator;
            _communicationEngine = communicationEngine;
            _deviceRepository = deviceRepository;
        }

        public async Task Execute()
        {
            _translator.Translate();
            Board = _translator.Board;
            DependencyGraph = _translator.DependencyGraph;


            Time = 0;
            Console.WriteLine(Board[0][1]);

            DependencyGraph.GenerateDotFile();

            // Reset the execution
            ContaminationMap = new byte[Board.Length, Board[0].Length];
            Agents.Clear();
            Agent.ResetSubstanceId();

            _router.Initialize(Board);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            List<BoardAction> boardActions = new List<BoardAction>();

            while (DependencyGraph.GetExecutableNodes().Count > 0)
            {
                foreach (var node in DependencyGraph.GetExecutableNodes())
                {
                    Console.WriteLine(node);
                }

                List<IDependencyNode> executableNodes = DependencyGraph.GetExecutableNodes();
                List<ICommand> commands = executableNodes.ConvertAll(node => node.Command);
                List<IDropletCommand> commandsToExecute = commands
                    .FindAll(c => c is IDropletCommand)
                    .ConvertAll(c => (IDropletCommand)c);

                commands.ForEach(c => c.Evaluate(Variables));

                List<IDropletCommand> movesToExecute = new List<IDropletCommand>();
                double? executionTime = Time;
                foreach (IDropletCommand command in commandsToExecute)
                {
                    switch (command)
                    {
                        case DropletDeclaration dropletCommand:
                            HandleDropletDeclaration(dropletCommand);
                            break;

                        case Move moveCommand:
                            moveCommand.Evaluate(Variables);
                            movesToExecute.Add(moveCommand);
                            break;

                        case Merge mergeCommand:
                            HandleMergeCommand(mergeCommand, movesToExecute, boardActions, ref executionTime, Agents);
                            break;

                        case SplitByRatio splitByRatioCommand:
                            HandleSplitByRatioCommand(splitByRatioCommand, movesToExecute, boardActions, executionTime, Agents);
                            break;

                        case SplitByVolume splitByVolumeCommand:
                            HandleSplitByVolumeCommand(splitByVolumeCommand, movesToExecute, boardActions, ref executionTime, Agents);
                            break;

                        case Store storeCommand:
                            if (_actionService.InPositionToStore(storeCommand, Agents, movesToExecute))
                            {
                                _storeService.StoreDroplet(storeCommand, Time);
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

                        case SensorCommand sensorCommand:
                            await HandleSensorCommand(sensorCommand, movesToExecute);
                            break;
                        case ActuatorCommand actuatorCommand:
                            //var statusCode = await _communicationEngine.SendActuatorRequest(actuatorCommand.ActuatorName, Time);
                            await HandleActuatorCommand(actuatorCommand);
                            break;

                        default:
                            Console.WriteLine($"Unknown dropletCommand: {command}");
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
                
                await SendActions(boardActions);

                Console.WriteLine($"Compiler time {Time}");
                boardActions.Clear();

            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

        }

        private void HandleDropletDeclaration(DropletDeclaration dropletCommand)
        {

            var contamintation = ContaminationMap[dropletCommand.PositionX, dropletCommand.PositionY];
            if (contamintation != 0)
            {
                throw new Exception($"Cannot declare a new droplet at Position {dropletCommand.PositionX}, {dropletCommand.PositionY} since it is already contaminated");
            }


            Agent agent = new Agent(dropletCommand.DropletName, dropletCommand.PositionX,
                dropletCommand.PositionY, dropletCommand.Volume);
            Agents.Add(dropletCommand.DropletName, agent);
            ContaminationMap = _contaminationService.ApplyContamination(agent, ContaminationMap);
        }

        private async Task HandleActuatorCommand(ActuatorCommand actuatorCommand)
        {
            if (!_deviceRepository.Actuators.TryGetValue(actuatorCommand.ActuatorName, out var actuator))
            {
                throw new Exception($"We could not find any actuator with name {actuatorCommand.ActuatorName}");
            }

            actuator.Arguments = actuatorCommand.KeyValuePairs;

            var invalidArguments = actuator.Arguments.Keys
                .Where(key => !actuator.ValidArguments.Contains(key))
                .ToList();

            if (invalidArguments.Any())
            {
                throw new ArgumentException($"Actuator {actuator.Name} had invalid arguments: [{string.Join(", ", invalidArguments)}]. Valid arguments are: [{String.Join(", ", actuator.ValidArguments.ToArray())}]");
            }

            await _communicationEngine.SendActuatorRequest(actuator, Time);
        }
        
        private async Task SendActions(List<BoardAction> boardActions)
        {
            if (boardActions.Count > 0)
            {
                await _communicationEngine.SendActions(boardActions);
            }
        }

        private async Task HandleSensorCommand(SensorCommand sensorCommand, List<IDropletCommand> movesToExecute)
        {
            if (_actionService.InPositionToSense(sensorCommand, Agents, movesToExecute))
            {
                _commandLifetimeService.StoreCommand(sensorCommand);
                var sensorValue = await _communicationEngine.SendSensorRequest(sensorCommand.SensorName, sensorCommand.Argument, Time);
                Console.WriteLine($"Sensor value is {sensorValue}");
                Variables[sensorCommand.VariableName] = sensorValue;
            }
        }

        private async Task HandleMixCommand(Mix mixCommand, List<IDropletCommand> movesToExecute)
        {
            if (_actionService.InPositionToMix(mixCommand, Agents, movesToExecute))
            {
                List<BoardAction> mixActions = new List<BoardAction>();
                mixActions.AddRange(_actionService.Mix(Agents, mixCommand, ContaminationMap, Time));
                _storeService.StoreDropletWithNameAndTime(mixCommand.DropletName, mixActions.Last().Time);
                
                await _communicationEngine.SendActions(mixActions);
            }
        }

        private void HandleMergeCommand(Merge mergeCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, ref double? executionTime, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(mergeCommand, agents))
            {
                var mergePositions = _schedulerService.ScheduleCommand(mergeCommand, agents,
                    ContaminationMap);
                if (_actionService.InPositionToMerge(mergeCommand, movesToExecute, mergePositions.Value, agents))
                {
                    _commandLifetimeService.StoreCommand(mergeCommand);
                    boardActions.AddRange(_actionService.Merge(agents, mergeCommand, ContaminationMap, Time));
                    executionTime = boardActions.Any() && boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time;
                }
            }
            else
            {
                _actionService.MoveMergeDropletToPosition(mergeCommand, movesToExecute, agents);
            }
        }

        private void HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, ref double? executionTime, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(splitByVolumeCommand, agents))
            {
                var splitPositions = _schedulerService.ScheduleCommand(splitByVolumeCommand, agents,
                    ContaminationMap);
                if (_actionService.InPositionToSplit(splitByVolumeCommand, movesToExecute, splitPositions.Value, agents))
                {
                    _commandLifetimeService.StoreCommand(splitByVolumeCommand);
                    boardActions.AddRange(_actionService.SplitByVolume(Agents, splitByVolumeCommand, ContaminationMap, Time, splitPositions.Value));
                    executionTime = boardActions.Any() ? boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time : Time;
                }
            }
            else
            {
                _actionService.MoveToSplitToFinalPositions(splitByVolumeCommand, movesToExecute, agents);
            }
        }
        

        private void HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, double? executionTime, Dictionary<string, Agent> agents)
        {
            SplitByVolume splitByVolumeCommand2 = new SplitByVolume(splitByRatioCommand.InputName, splitByRatioCommand.OutputName1,
                splitByRatioCommand.OutputName2, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1,
                splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2,
                Agents.ContainsKey(splitByRatioCommand.InputName) ? Agents[splitByRatioCommand.InputName].Volume * splitByRatioCommand.Ratio : 1);
            HandleSplitByVolumeCommand(splitByVolumeCommand2, movesToExecute, boardActions, ref executionTime, agents);
        }

    }
}
