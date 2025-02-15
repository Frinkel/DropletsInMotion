﻿using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.Factories;
using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Translation.Services;

namespace DropletsInMotion.Application.Services
{
    public class ActionService : IActionService
    {

        private readonly MoveHandler _moveHandler;
        private readonly ITemplateService _templateService;
        private readonly IContaminationService _contaminationService;
        private readonly IStoreService _storeService;
        private readonly ICommandLifetimeService _commandLifetimeService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IPlatformService _platformService;
        private readonly IPlatformRepository _platformRepository;
        private readonly IAgentFactory _agentFactory;

        public ActionService(ITemplateService templateService, IContaminationService contaminationService, IStoreService storeService, 
                             ICommandLifetimeService commandLifetimeService, IDeviceRepository deviceRepository, ITemplateRepository templateRepository,
                             IPlatformService platformService, IPlatformRepository platformRepository, IAgentFactory agentFactory)
        {
            _templateService = templateService;
            _contaminationService = contaminationService;
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
            _deviceRepository = deviceRepository;
            _platformService = platformService;
            _platformRepository = platformRepository;
            _agentFactory = agentFactory;
            _moveHandler = new MoveHandler(_templateService, templateRepository, platformRepository);

        }

        public List<BoardAction> Merge(Dictionary<string, Agent> agents, Merge mergeCommand, List<int>[,] contaminationMap, double time, ScheduledPosition mergePositions)
        {
            //Merge
            Droplet inputDroplet1 = agents[mergeCommand.InputName1]
                                    ?? throw new CommandException($"No droplet found with name {mergeCommand.InputName1}.", mergeCommand);

            Droplet inputDroplet2 = agents[mergeCommand.InputName2]
                                    ?? throw new CommandException($"No droplet found with name {mergeCommand.InputName2}.", mergeCommand);


            int outPutDropletX = mergePositions.SingularX;
            int outPutDropletY = mergePositions.SingularY;
            Droplet outputDroplet = new Droplet(mergeCommand.OutputName, outPutDropletX, outPutDropletY,
                inputDroplet1.Volume + inputDroplet2.Volume);


            List<BoardAction> mergeActions = mergePositions.Template.Apply(_platformService.Board[mergePositions.OriginX][mergePositions.OriginY].Id, time, 1);

            Agent inputAgent1 = agents[inputDroplet1.DropletName];
            Agent inputAgent2 = agents[inputDroplet2.DropletName];


            var newSubstanceId = _contaminationService.GetResultingSubstanceId(inputAgent1.SubstanceId, inputAgent2.SubstanceId);
            Agent newAgent = _agentFactory.CreateAgent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume, newSubstanceId);

            agents.Remove(inputDroplet1.DropletName);
            agents.Remove(inputDroplet2.DropletName);

            agents.Add(newAgent.DropletName, newAgent);

            // Map template to droplets
            mergePositions.Template = MapTemplateToDroplets(mergePositions, inputDroplet1, inputDroplet2, newAgent);

            // Apply contamination
            _contaminationService.ApplyContaminationMerge(inputAgent1, inputAgent2, newAgent, mergePositions, contaminationMap);

            return mergeActions;
        }


        private MergeTemplate MapTemplateToDroplets(ScheduledPosition mergePositions, Droplet inputDroplet1, Droplet inputDroplet2, Agent newAgent)
        {
            // Map the cluster ID to the droplet ID
            var mergeTemplate = mergePositions.Template as MergeTemplate;
            if (mergeTemplate == null) throw new TemplateException("The template was not on the expected type", mergePositions.Template);
            var templateCopy = mergeTemplate.DeepCopy();

            var initialPositions = templateCopy.InitialPositions;
            var positionToDropletMap = new Dictionary<string, Droplet>();

            foreach (var initialPosition in initialPositions)
            {
                var initialX = initialPosition.Value.x + mergePositions.OriginX;
                var initialY = initialPosition.Value.y + mergePositions.OriginY;

                if (inputDroplet1.PositionX == initialX && inputDroplet1.PositionY == initialY)
                {
                    positionToDropletMap[initialPosition.Key] = inputDroplet1;
                }
                else if (inputDroplet2.PositionX == initialX && inputDroplet2.PositionY == initialY)
                {
                    positionToDropletMap[initialPosition.Key] = inputDroplet2;
                }
                else
                {
                    throw new DropletNotFoundException($"None of the merge droplets ({inputDroplet1.DropletName}, {inputDroplet2.DropletName}) matched the initial positions in the scheduled merge position ({mergePositions.OriginX}, {mergePositions.OriginY})"); //TODO: Is it origin here?
                }
            }


            foreach (var block in templateCopy.Blocks)
            {
                var mappedBlock = new Dictionary<string, List<(int x, int y)>>();

                foreach (var keyValuePair in block)
                {
                    string dropletName;

                    if (block.Count == 1)
                    {
                        dropletName = newAgent.DropletName;
                    }
                    else
                    {
                        if (positionToDropletMap.TryGetValue(keyValuePair.Key, out var droplet))
                        {
                            dropletName = droplet.DropletName;
                        }
                        else
                        {
                            throw new DropletNotFoundException($"The value {keyValuePair.Key} did not map to any droplet in {mergePositions.Template.Name}");
                        }
                    }

                    mappedBlock[dropletName] = keyValuePair.Value;
                }

                block.Clear();
                foreach (var kvp in mappedBlock)
                {
                    block.Add(kvp.Key, kvp.Value);
                }
            }

            return templateCopy;
        }

        public List<BoardAction> SplitByVolume(
             Dictionary<string, Agent> agents,
             SplitByVolume splitCommand,
             List<int>[,] contaminationMap,
             double time,
             ScheduledPosition splitPositions)
        {
            Agent inputDroplet = agents[splitCommand.InputName]
                                   ?? throw new CommandException($"No droplet found with name {splitCommand.InputName}.", splitCommand);

            // Ensure that the output droplet names are valid and unique
            if (agents.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
            {
                throw new CommandException($"Droplet with name {splitCommand.OutputName1} already exists.", splitCommand);
            }
            if (agents.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
            {
                throw new CommandException($"Droplet with name {splitCommand.OutputName2} already exists.", splitCommand);
            }
            if (splitCommand.OutputName2 == splitCommand.OutputName1)
            {
                throw new CommandException($"Droplets with the same names cannot be split.", splitCommand);
            }


            bool isBetweenHorizontally = (inputDroplet.PositionX == (splitPositions.X1 + splitPositions.X2) / 2) &&
                                         (inputDroplet.PositionY == splitPositions.Y1 && inputDroplet.PositionY == splitPositions.Y2);

            bool isBetweenVertically = (inputDroplet.PositionY == (splitPositions.Y1 + splitPositions.Y2) / 2) &&
                                       (inputDroplet.PositionX == splitPositions.X1 && inputDroplet.PositionX == splitPositions.X2);

            if (!isBetweenHorizontally && !isBetweenVertically)
            {
                throw new RuntimeException("Input droplet is not positioned between the specified split positions.");
            }


            Droplet outputDroplet1 = new Droplet(splitCommand.OutputName1, splitPositions.X1, splitPositions.Y1, inputDroplet.Volume - splitCommand.Volume);
            Droplet outputDroplet2 = new Droplet(splitCommand.OutputName2, splitPositions.X2, splitPositions.Y2, splitCommand.Volume);

            Agent newAgent1 = _agentFactory.CreateAgent(outputDroplet1.DropletName, outputDroplet1.PositionX,
                outputDroplet1.PositionY, outputDroplet1.Volume, agents[inputDroplet.DropletName].SubstanceId);
            Agent newAgent2 = _agentFactory.CreateAgent(outputDroplet2.DropletName, outputDroplet2.PositionX, 
                outputDroplet2.PositionY, outputDroplet2.Volume, agents[inputDroplet.DropletName].SubstanceId);

            agents.Remove(inputDroplet.DropletName);

            agents[outputDroplet1.DropletName] = newAgent1;
            agents[outputDroplet2.DropletName] = newAgent2;

            // Apply contamination
            _contaminationService.ApplyContaminationSplit(inputDroplet, splitPositions, contaminationMap);

            List<BoardAction> splitActions = splitPositions.Template.Apply(_platformService.Board[inputDroplet.PositionX][inputDroplet.PositionY].Id, time, 1);

            return splitActions;
        }

        public List<BoardAction> Mix(Dictionary<string, Agent> agents, Mix mixCommand, List<int>[,] contaminationMap, double compilerTime)
        {
            Agent inputDroplet = agents[mixCommand.DropletName]
                                 ?? throw new CommandException($"No droplet found with name {mixCommand.DropletName}.", mixCommand);
            if (_contaminationService.IsAreaContaminated(contaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX,
                    mixCommand.PositionY, mixCommand.Width, mixCommand.Height))
            {
                throw new ContaminationException($"Mix not possible Area is contaminated.", contaminationMap);
            }
            List<BoardAction> mixActions = new List<BoardAction>();
            double time1 = compilerTime;

            double scaleFactor = (int)(inputDroplet.Volume / _platformRepository.MinimumMovementVolume);

            mixActions.AddRange(_moveHandler.Unravel(inputDroplet, time1));

            for (int i = 0; i < mixCommand.RepeatTimes; i++)
            {
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX + mixCommand.Width, inputDroplet.PositionY, ref time1, scaleFactor));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY + mixCommand.Height, ref time1, scaleFactor));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX - mixCommand.Width, inputDroplet.PositionY, ref time1, scaleFactor));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY - mixCommand.Height, ref time1, scaleFactor));
            }


            // Apply contamination
            _contaminationService.UpdateContaminationArea(contaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX - 1,
                mixCommand.PositionY - 1, mixCommand.Width + 2, mixCommand.Height + 2);

            mixActions = mixActions.OrderBy(b => b.Time).ToList();
            List<BoardAction> ravelActions = _moveHandler.Ravel(inputDroplet, mixActions.Last().Time);
            mixActions.AddRange(ravelActions);

            mixActions = mixActions.OrderBy(b => b.Time).ToList();

            BoardActionUtils.FilterBoardActions(ravelActions, mixActions);

            return mixActions;
        }

        public bool InPositionToMix(Mix mixCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute)
        {
            if (_storeService.ContainsDroplet(mixCommand.DropletName))
            {
                return false;
            }

            if (!agents.TryGetValue(mixCommand.DropletName, out var inputDroplet))
            {
                throw new CommandException($"No droplet found with name {mixCommand.DropletName}.", mixCommand);
            }

            if (inputDroplet.PositionX == mixCommand.PositionX && inputDroplet.PositionY == mixCommand.PositionY)
            {
                return true;
            }
            movesToExecute.Add(new Move(mixCommand.DropletName, mixCommand.PositionX, mixCommand.PositionY));
            return false;
        }

        public bool InPositionToSense(SensorCommand sensorCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute)
        {

            if (!agents.TryGetValue(sensorCommand.DropletName, out var inputDroplet))
            {
                throw new CommandException($"No droplet found with name {sensorCommand.DropletName}.", sensorCommand);
            }

            if (!_deviceRepository.Sensors.TryGetValue(sensorCommand.SensorName, out var sensor))
            {
                throw new CommandException($"No sensor found with name {sensorCommand.SensorName}.", sensorCommand);
            }

            if (inputDroplet.PositionX == sensor.CoordinateX && inputDroplet.PositionY == sensor.CoordinateY)
            {
                return true;
            }
            movesToExecute.Add(new Move(sensorCommand.DropletName, sensor.CoordinateX, sensor.CoordinateY));
            return false;
        }

        public bool InPositionToWaste(Waste wasteCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute)
        {
            if (!agents.TryGetValue(wasteCommand.DropletName, out var inputDroplet))
            {
                throw new CommandException($"No droplet found with name {wasteCommand.DropletName}.", wasteCommand);
            }

            if (inputDroplet.PositionX == wasteCommand.PositionX && inputDroplet.PositionY == wasteCommand.PositionY)
            {
                return true;
            }
            movesToExecute.Add(new Move(wasteCommand.DropletName, wasteCommand.PositionX, wasteCommand.PositionY));
            return false;
        }


        public bool InPositionToStore(Store storeCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute)
        {
            if (!agents.TryGetValue(storeCommand.DropletName, out var inputDroplet))
            {
                throw new CommandException($"No droplet found with name {storeCommand.DropletName}.", storeCommand);
            }

            if (storeCommand.StoreInPosition || inputDroplet.PositionX == storeCommand.PositionX && inputDroplet.PositionY == storeCommand.PositionY)
            {
                return true;
            }
            movesToExecute.Add(new Move(storeCommand.DropletName, storeCommand.PositionX, storeCommand.PositionY));
            return false;
        }


        public bool DropletsExistAndCommandInProgress(IDropletCommand dropletCommand, Dictionary<string, Agent> agents)
        {
            if (_commandLifetimeService.CanExecuteCommand(dropletCommand))
            {
                var inputDroplets = dropletCommand.GetInputDroplets();
                foreach (var inputDroplet in inputDroplets)
                {
                    if (!agents.ContainsKey(inputDroplet))
                    {
                        throw new CommandException($"No droplet found with name {inputDroplet} for dropletCommand {dropletCommand}.", dropletCommand);
                    }
                }
                return true;
            }

            return false;
        }

        public void MoveMergeDropletToPosition(Merge mergeCommand, List<IDropletCommand> movesToExecute, Dictionary<string, Agent> agents)
        {

            if (!agents.TryGetValue(mergeCommand.OutputName, out var outDroplet))
            {
                throw new CommandException($"No droplet found with name {mergeCommand.OutputName}.", mergeCommand);
            }

            var moveCommand = new Move(mergeCommand.OutputName, mergeCommand.PositionX, mergeCommand.PositionY);
            movesToExecute.Add(moveCommand);
        }


        public bool InPositionToMerge(Merge mergeCommand, List<IDropletCommand> movesToExecute, ScheduledPosition mergePositions, Dictionary<string, Agent> agents)
        {
            var inputDroplet1 = agents[mergeCommand.InputName1];
            var inputDroplet2 = agents[mergeCommand.InputName2];

            bool areInPosition = inputDroplet1.PositionX == mergePositions.X1 &&
                                 inputDroplet1.PositionY == mergePositions.Y1 &&
                                 inputDroplet2.PositionX == mergePositions.X2 &&
                                 inputDroplet2.PositionY == mergePositions.Y2;

            // If the droplets are already in position, return true
            if (areInPosition)
            {
                return true;
            }

            // Move inputDroplet1 to be next to the merge position
            if (inputDroplet1.PositionX != mergePositions.X1 || inputDroplet1.PositionY != mergePositions.Y1)
            {
                var moveCommand = new Move(inputDroplet1.DropletName, mergePositions.X1, mergePositions.Y1);
                movesToExecute.Add(moveCommand);
            }

            // Move inputDroplet2 to be next to the merge position
            if (inputDroplet2.PositionX != mergePositions.X2 || inputDroplet2.PositionY != mergePositions.Y2)
            {
                var moveCommand = new Move(inputDroplet2.DropletName, mergePositions.X2, mergePositions.Y2);
                movesToExecute.Add(moveCommand);

            }

            return false;
        }

        public bool InPositionToSplit(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            ScheduledPosition splitPositions,
            Dictionary<string, Agent> agents)
        {

            if (agents.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
            {
                throw new CommandException($"Droplet with name {splitCommand.OutputName1} already exists.", splitCommand);
            }

            if (agents.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
            {
                throw new CommandException($"Droplet with name {splitCommand.OutputName2} already exists.", splitCommand);
            }

            if (splitCommand.OutputName2 == splitCommand.OutputName1)
            {
                throw new CommandException($"Droplet with the same names can not be split.", splitCommand);
            }

            int splitPositionX = (splitPositions.X1 + splitPositions.X2) / 2;
            int splitPositionY = (splitPositions.Y1 + splitPositions.Y2) / 2;

            Droplet splitDroplet = agents[splitCommand.InputName];

            if (splitDroplet.PositionX == splitPositionX && splitDroplet.PositionY == splitPositionY)
            {
                return true;
            }

            movesToExecute.Add(new Move(splitCommand.InputName, splitPositionX, splitPositionY));
            return false;
        }


        public void MoveToSplitToFinalPositions(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            Dictionary<string, Agent> agents)
        {

            if (agents.TryGetValue(splitCommand.OutputName1, out Agent outputDroplet1))
            {
                if (outputDroplet1.PositionX != splitCommand.PositionX1 || outputDroplet1.PositionY != splitCommand.PositionY1)
                {
                    movesToExecute.Add(new Move(splitCommand.OutputName1, splitCommand.PositionX1, splitCommand.PositionY1));
                }
            }
            if (agents.TryGetValue(splitCommand.OutputName2, out Agent outputDroplet2))
            {
                if (outputDroplet2.PositionX != splitCommand.PositionX2 || outputDroplet2.PositionY != splitCommand.PositionY2)
                {
                    movesToExecute.Add(new Move(splitCommand.OutputName2, splitCommand.PositionX2, splitCommand.PositionY2));
                }
            }

        }

    }
}
