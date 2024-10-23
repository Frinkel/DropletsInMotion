using System.Reflection.Metadata;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Communication;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
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
        private readonly ITemplateRepository _templateRepository;
        private readonly IPlatformRepository _platformRepository;

        public ExecutionEngine(IContaminationService contaminationService, ISchedulerService schedulerService, 
                                IStoreService storeService, ICommandLifetimeService commandLifetimeService, ITimeService timeService, 
                                IActionService actionService, IRouterService routerService, IDependencyService dependencyService, 
                                ITemplateService templateService, ITranslator translator, ICommunicationEngine communicationEngine, IDeviceRepository deviceRepository, ITemplateRepository templateRepository, IPlatformRepository platformRepository)
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
            _templateRepository = templateRepository;
            _platformRepository = platformRepository;
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
                double executionTime = Time;
                foreach (IDropletCommand command in commandsToExecute)
                {
                    switch (command)
                    {
                        case DropletDeclaration dropletCommand:
                            HandleDropletDeclaration(dropletCommand, boardActions);
                            break;

                        case Dispense dispenseCommand:
                            HandleDispense(dispenseCommand, boardActions, Agents);
                            break;

                        case Move moveCommand:
                            moveCommand.Evaluate(Variables);
                            movesToExecute.Add(moveCommand);
                            break;

                        case Merge mergeCommand:
                            HandleMergeCommand(mergeCommand, movesToExecute, boardActions, Agents);
                            break;

                        case SplitByRatio splitByRatioCommand:
                            HandleSplitByRatioCommand(splitByRatioCommand, movesToExecute, boardActions, Agents);
                            break;

                        case SplitByVolume splitByVolumeCommand:
                            HandleSplitByVolumeCommand(splitByVolumeCommand, movesToExecute, boardActions, Agents);
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
                            Console.ReadLine();
                            break;

                        case Wait waitCommand:
                            executionTime = waitCommand.Time + Time;
                            break;

                        case Waste wasteCommand:
                            HandleWasteCommand(wasteCommand, movesToExecute);
                            break;

                        case SensorCommand sensorCommand:
                            await HandleSensorCommand(sensorCommand, movesToExecute);
                            break;
                        case ActuatorCommand actuatorCommand:
                            await HandleActuatorCommand(actuatorCommand);
                            break;

                        default:
                            Console.WriteLine($"Unknown dropletCommand: {command}");
                            break;
                    }
                }

                boardActions = boardActions.OrderBy(b => b.Time).ToList();
                if(boardActions.Count > 0) { executionTime = boardActions.Last().Time; }

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

                Console.WriteLine($"Compiler time {Time}");

                await SendActions(boardActions);

                boardActions.Clear();

            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

        }

        private void HandleWasteCommand(Waste wasteCommand, List<IDropletCommand> movesToExecute)
        {
            if (Agents.ContainsKey(wasteCommand.DropletName))
            {
                if (_actionService.InPositionToWaste(wasteCommand, Agents, movesToExecute))
                {
                    Agents.Remove(wasteCommand.DropletName);

                }
            }
            else
            {
                throw new Exception($"Cannot waste droplet {wasteCommand.DropletName} since it does not exist");
            }
        }

        private void HandleDispense(Dispense dispenseCommand, List<BoardAction> boardActions, Dictionary<string, Agent> agents)
        {
            if (!_deviceRepository.Reservoirs.TryGetValue(dispenseCommand.ReservoirName, out Reservoir reservoir))
            {
                throw new Exception($"No reservoir with name {dispenseCommand.ReservoirName}");
            }

            foreach (var kvp in reservoir.DispenseSequence)
            {
                
                BoardAction b = new BoardAction(Convert.ToInt32(kvp["id"]), Convert.ToInt32(kvp["status"]), kvp["time"] + Time);
                boardActions.Add(b);
            }

            // Update time
            //executionTime = boardActions.Any() && boardActions.Last().Time > executionTime ? boardActions.Last().Time : Time;

            Agent agent = new Agent(dispenseCommand.DropletName, reservoir.OutputX,
                reservoir.OutputY, dispenseCommand.Volume); // TODO: the reservoir should contain a substance id?
            Agents.Add(dispenseCommand.DropletName, agent);
            ContaminationMap = _contaminationService.ApplyContamination(agent, ContaminationMap);

            DeclareTemplate declareTemplate = _templateRepository?
                .DeclareTemplates?
                .Find(t => t.MinSize <= agent.Volume && agent.Volume < t.MaxSize);

            if (declareTemplate == null)
            {
                throw new Exception($"Cannot dispense a new droplet at Position {reservoir.OutputX}, {reservoir.OutputY} since there is not template");
            }

            List<BoardAction> initialElectrodes = declareTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, Time, 1);
            boardActions.AddRange(initialElectrodes);

        }

        private void HandleDropletDeclaration(DropletDeclaration dropletCommand, List<BoardAction> boardActions)
        {

            var contamintation = ContaminationMap[dropletCommand.PositionX, dropletCommand.PositionY];
            if (contamintation != 0)
            {
                throw new Exception($"Cannot declare a new droplet at Position {dropletCommand.PositionX}, {dropletCommand.PositionY} since it is already contaminated");
            }

            Agent agent = new Agent(dropletCommand.DropletName, dropletCommand.PositionX,
                dropletCommand.PositionY, dropletCommand.Volume);
            Agents.Add(dropletCommand.DropletName, agent);
            ContaminationMap = _contaminationService.ApplyContaminationWithSize(agent, ContaminationMap);

            DeclareTemplate declareTemplate = _templateRepository?
                .DeclareTemplates?
                .Find(t => t.MinSize <= agent.Volume && agent.Volume < t.MaxSize);

            if (declareTemplate == null)
            {
                throw new Exception($"Cannot declare a new droplet at Position {dropletCommand.PositionX}, {dropletCommand.PositionY} since there is not template");
            }

            List<BoardAction> initialElectrodes = declareTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, Time, 1);
            boardActions.AddRange(initialElectrodes);
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
                //boardActions.ForEach(b => Console.WriteLine(b));
                var actualTime = await _communicationEngine.SendTimeRequest();
                var boardActionTime = boardActions.First().Time;

                Console.WriteLine(actualTime);

                // Handle time desync
                // TODO: Check if mix sends its own request, we should do it here!
                if (boardActionTime <= actualTime) // TODO: Do we need +1 here?
                {
                    var timeDifference = actualTime - boardActionTime + 1; // TODO: how can we a good buffer?
                    boardActions.ForEach(b => b.Time += timeDifference);
                    Time = boardActions.Last().Time; 
                }



                await _communicationEngine.SendActions(boardActions);
            }
        }

        private async Task HandleSensorCommand(SensorCommand sensorCommand, List<IDropletCommand> movesToExecute)
        {
            if (_actionService.InPositionToSense(sensorCommand, Agents, movesToExecute))
            {
                _commandLifetimeService.StoreCommand(sensorCommand);

                if (!_deviceRepository.Sensors.TryGetValue(sensorCommand.SensorName, out var sensor))
                {
                    throw new Exception($"We could not find any actuator with name {sensorCommand.SensorName}");
                }

                if (!sensor.ArgumentHandlers.TryGetValue(sensorCommand.Argument, out SensorHandler? handler))
                {
                    throw new Exception($"We could not find any argument {sensorCommand.Argument} in sensor {sensor.Name}");
                }

                var sensorValue = await _communicationEngine.SendSensorRequest(sensor, handler, Time);
                //Console.WriteLine($"Sensor value is {sensorValue}");
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

        private void HandleMergeCommand(Merge mergeCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(mergeCommand, agents))
            {
                Agent mergeAgent1 = agents[mergeCommand.InputName1];
                Agent mergeAgent2 = agents[mergeCommand.InputName2];
                double agentVolume1 = mergeAgent1.Volume;
                double agentVolume2 = mergeAgent2.Volume;


                List<ITemplate> eligibleMergeTemplates = _templateRepository?
                    .MergeTemplates?
                    .FindAll(t => t.MinSize <= agentVolume1 + agentVolume2 && agentVolume1 + agentVolume2 < t.MaxSize)
                    ?.Cast<ITemplate>()
                    .ToList() ?? new List<ITemplate>();

                if (!eligibleMergeTemplates.Any())
                {
                    throw new Exception($"There were no eligible merge templates for command {mergeCommand}");
                }


                var mergePositions = _schedulerService.ScheduleCommand(mergeCommand, agents, ContaminationMap, eligibleMergeTemplates);
                if (_actionService.InPositionToMerge(mergeCommand, movesToExecute, mergePositions, agents))
                {
                    _commandLifetimeService.StoreCommand(mergeCommand);
                    boardActions.AddRange(_actionService.Merge(agents, mergeCommand, ContaminationMap, Time, mergePositions));
                }
            }
            else
            {
                _actionService.MoveMergeDropletToPosition(mergeCommand, movesToExecute, agents);
            }
        }

        private void HandleSplitByVolumeCommand(SplitByVolume splitByVolumeCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, Dictionary<string, Agent> agents)
        {
            if (_actionService.DropletsExistAndCommandInProgress(splitByVolumeCommand, agents))
            {
                // Find eligible templates
                Agent splitAgent = agents[splitByVolumeCommand.InputName];
                double agentVolume = splitAgent.Volume;
                double ratio = splitByVolumeCommand.Volume / splitAgent.Volume;

                List<ITemplate> eligibleSplitTemplates = new List<ITemplate>();

                // Find the relation between cluster ids and the agents
                foreach (var template in _templateRepository.SplitTemplates)
                {
                    if ((template.MinSize <= agentVolume && agentVolume < template.MaxSize &&
                          Math.Abs(template.Ratio - ratio) < 0.1))
                    {
                        
                        if (Math.Abs(template.RatioRelation.First().Value - ratio) < 0.1) // TODO: Should this tolerance be user defined
                        {
                            SplitTemplate t = template.DeepCopy();

                            Dictionary<string, (int x, int y)>
                                finalPositions = new Dictionary<string, (int x, int y)>();

                            finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                            finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                            t.FinalPositions = finalPositions;

                            eligibleSplitTemplates.Add(t);
                        }
                        else
                        {
                            SplitTemplate t = template.DeepCopy();

                            Dictionary<string, (int x, int y)>
                                finalPositions = new Dictionary<string, (int x, int y)>();

                            finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                            finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                            t.FinalPositions = finalPositions;

                            eligibleSplitTemplates.Add(t);
                        }
                    }
                }


                if (!eligibleSplitTemplates.Any())
                {
                    throw new Exception($"There were no eligible split templates for command {splitByVolumeCommand}");
                }

                var splitPositions = _schedulerService.ScheduleCommand(splitByVolumeCommand, agents, ContaminationMap, eligibleSplitTemplates);

                if (_actionService.InPositionToSplit(splitByVolumeCommand, movesToExecute, splitPositions, agents))
                {
                    _commandLifetimeService.StoreCommand(splitByVolumeCommand);
                    boardActions.AddRange(_actionService.SplitByVolume(Agents, splitByVolumeCommand, ContaminationMap, Time, splitPositions));
                }
            }
            else
            {
                _actionService.MoveToSplitToFinalPositions(splitByVolumeCommand, movesToExecute, agents);
            }
        }
        

        private void HandleSplitByRatioCommand(SplitByRatio splitByRatioCommand, List<IDropletCommand> movesToExecute, List<BoardAction> boardActions, Dictionary<string, Agent> agents)
        {
            SplitByVolume splitByVolumeCommand2 = new SplitByVolume(splitByRatioCommand.InputName, splitByRatioCommand.OutputName1,
                splitByRatioCommand.OutputName2, splitByRatioCommand.PositionX1, splitByRatioCommand.PositionY1,
                splitByRatioCommand.PositionX2, splitByRatioCommand.PositionY2,
                Agents.ContainsKey(splitByRatioCommand.InputName) ? Agents[splitByRatioCommand.InputName].Volume * splitByRatioCommand.Ratio : 1);
            HandleSplitByVolumeCommand(splitByVolumeCommand2, movesToExecute, boardActions, agents);
        }

    }
}
