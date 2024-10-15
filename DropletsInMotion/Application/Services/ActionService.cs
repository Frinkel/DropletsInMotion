using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Communication;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Communication.Services;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;

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


        public ActionService(ITemplateService templateService, IContaminationService contaminationService, IStoreService storeService, ICommandLifetimeService commandLifetimeService, IDeviceRepository deviceRepository)
        {
            _templateService = templateService;
            _contaminationService = contaminationService;
            _moveHandler = new MoveHandler(_templateService);
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
            _deviceRepository = deviceRepository;
        }

        public List<BoardAction> Merge(Dictionary<string, Agent> agents, Merge mergeCommand, byte[,] contaminationMap, double time)
        {
            // Add logic for processing the Merge dropletCommand
            //Console.WriteLine($"Merging droplets with IDs: {mergeCommand.InputName1}, {mergeCommand.InputName2}");


            //Merge
            Droplet inputDroplet1 = agents[mergeCommand.InputName1]
                                    ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");

            Droplet inputDroplet2 = agents[mergeCommand.InputName2]
                                    ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");


            List<BoardAction> mergeActions = new List<BoardAction>();

            int outPutDropletX = (inputDroplet1.PositionX + inputDroplet2.PositionX) / 2;
            int outPutDropletY = (inputDroplet1.PositionY + inputDroplet2.PositionY) / 2;
            Droplet outputDroplet = new Droplet(mergeCommand.OutputName, outPutDropletX, outPutDropletY,
                inputDroplet1.Volume + inputDroplet2.Volume);

            if (Math.Abs(inputDroplet1.PositionX - inputDroplet2.PositionX) == 2 && inputDroplet1.PositionY == inputDroplet2.PositionY)
            {
                mergeActions.AddRange(_templateService.ApplyTemplate("mergeHorizontal", outputDroplet, time));

            }
            else if (Math.Abs(inputDroplet1.PositionY - inputDroplet2.PositionY) == 2 && inputDroplet1.PositionX == inputDroplet2.PositionX)
            {
                mergeActions.AddRange(_templateService.ApplyTemplate("mergeVertical", outputDroplet, time));
            }
            else
            {
                throw new InvalidOperationException("Droplets are not in position to merge");
            }


            Agent newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume);

            if (agents[inputDroplet1.DropletName].SubstanceId == agents[inputDroplet2.DropletName].SubstanceId)
            {
                newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume, agents[inputDroplet1.DropletName].SubstanceId);
            }

            agents.Remove(inputDroplet1.DropletName);
            agents.Remove(inputDroplet2.DropletName);

            agents.Add(newAgent.DropletName, newAgent);

            _contaminationService.ApplyContaminationMerge(newAgent, contaminationMap);
            _contaminationService.PrintContaminationState(contaminationMap);
            Console.WriteLine(outputDroplet);
            return mergeActions;
        }

        public List<BoardAction> SplitByVolume(
             Dictionary<string, Agent> agents,
             SplitByVolume splitCommand,
             byte[,] contaminationMap,
             double time,
             ((int optimalX, int optimalY), (int optimalX, int optimalY)) splitPositions)
        {
            Droplet inputDroplet = agents[splitCommand.InputName]
                                   ?? throw new InvalidOperationException($"No droplet found with name {splitCommand.InputName}.");

            // Ensure that the output droplet names are valid and unique
            if (agents.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
            {
                throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName1} already exists.");
            }
            if (agents.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
            {
                throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName2} already exists.");
            }
            if (splitCommand.OutputName2 == splitCommand.OutputName1)
            {
                throw new InvalidOperationException($"Droplets with the same names cannot be split.");
            }

            var (firstSplitPosition, secondSplitPosition) = splitPositions;

            bool isBetweenHorizontally = (inputDroplet.PositionX == (firstSplitPosition.optimalX + secondSplitPosition.optimalX) / 2) &&
                                         (inputDroplet.PositionY == firstSplitPosition.optimalY && inputDroplet.PositionY == secondSplitPosition.optimalY);

            bool isBetweenVertically = (inputDroplet.PositionY == (firstSplitPosition.optimalY + secondSplitPosition.optimalY) / 2) &&
                                       (inputDroplet.PositionX == firstSplitPosition.optimalX && inputDroplet.PositionX == secondSplitPosition.optimalX);

            if (!isBetweenHorizontally && !isBetweenVertically)
            {
                throw new InvalidOperationException("Input droplet is not positioned between the specified split positions.");
            }

            Droplet outputDroplet1 = new Droplet(splitCommand.OutputName1, firstSplitPosition.optimalX, firstSplitPosition.optimalY, inputDroplet.Volume - splitCommand.Volume);
            Droplet outputDroplet2 = new Droplet(splitCommand.OutputName2, secondSplitPosition.optimalX, secondSplitPosition.optimalY, splitCommand.Volume);

            string templateName;
            if (isBetweenHorizontally)
            {
                templateName = "splitHorizontal";
            }
            else if (isBetweenVertically)
            {
                templateName = "splitVertical";
            }
            else
            {
                throw new InvalidOperationException("The split could not be determined to be horizontal or vertical.");
            }

            Agent newAgent1 = new Agent(outputDroplet1.DropletName, outputDroplet1.PositionX, outputDroplet1.PositionY, outputDroplet1.Volume, agents[inputDroplet.DropletName].SubstanceId);
            Agent newAgent2 = new Agent(outputDroplet2.DropletName, outputDroplet2.PositionX, outputDroplet2.PositionY, outputDroplet2.Volume, agents[inputDroplet.DropletName].SubstanceId);
            agents.Remove(inputDroplet.DropletName);

            agents[outputDroplet1.DropletName] = newAgent1;
            agents[outputDroplet2.DropletName] = newAgent2;
            _contaminationService.ApplyContamination(newAgent1, contaminationMap);
            _contaminationService.ApplyContamination(newAgent2, contaminationMap);

            List<BoardAction> splitActions = new List<BoardAction>();
            splitActions.AddRange(_templateService.ApplyTemplate(templateName, inputDroplet, time));

            return splitActions;
        }

        public List<BoardAction> Mix(Dictionary<string, Agent> agents, Mix mixCommand, byte[,] contaminationMap, double compilerTime)
        {
            Agent inputDroplet = agents[mixCommand.DropletName]
                                 ?? throw new InvalidOperationException($"No droplet found with name {mixCommand.DropletName}.");
            if (_contaminationService.IsAreaContaminated(contaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX,
                    mixCommand.PositionY, mixCommand.Width, mixCommand.Height))
            {
                throw new InvalidOperationException($"Mix not possible Area is contaminated.");
            }
            List<BoardAction> mixActions = new List<BoardAction>();
            double time1 = compilerTime;

            for (int i = 0; i < mixCommand.RepeatTimes; i++)
            {
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX + mixCommand.Width, inputDroplet.PositionY, ref time1));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY + mixCommand.Height, ref time1));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX - mixCommand.Width, inputDroplet.PositionY, ref time1));
                mixActions.AddRange(_moveHandler.MoveDroplet(inputDroplet, inputDroplet.PositionX, inputDroplet.PositionY - mixCommand.Height, ref time1));
            }
            Console.WriteLine("-----------------------------------------------------");
            _contaminationService.PrintContaminationState(contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, inputDroplet.SubstanceId, mixCommand.PositionX - 1,
                mixCommand.PositionY - 1, mixCommand.Width + 2, mixCommand.Height + 2);
            _contaminationService.PrintContaminationState(contaminationMap);
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
                throw new InvalidOperationException($"No droplet found with name {mixCommand.DropletName}.");
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
                throw new InvalidOperationException($"No droplet found with name {sensorCommand.DropletName}.");
            }

            if (!_deviceRepository.Sensors.TryGetValue(sensorCommand.SensorName, out var sensor))
            {
                throw new InvalidOperationException($"No droplet found with name {sensorCommand.DropletName}.");
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
                throw new InvalidOperationException($"No droplet found with name {wasteCommand.DropletName}.");
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
                throw new InvalidOperationException($"No droplet found with name {storeCommand.DropletName}.");
            }

            if (inputDroplet.PositionX == storeCommand.PositionX && inputDroplet.PositionY == storeCommand.PositionY)
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
                        throw new InvalidOperationException($"No droplet found with name {inputDroplet} for dropletCommand {dropletCommand}.");
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
                throw new InvalidOperationException($"No droplet found with name {mergeCommand.OutputName}.");
            }

            var moveCommand = new Move(mergeCommand.OutputName, mergeCommand.PositionX, mergeCommand.PositionY);
            movesToExecute.Add(moveCommand);
            Console.WriteLine($"Move dropletCommand added for droplet 2: {moveCommand}");
        }


        public bool InPositionToMerge(Merge mergeCommand, List<IDropletCommand> movesToExecute, ((int optimalX, int optimalY), (int optimalX, int optimalY)) mergePositions, Dictionary<string, Agent> agents)
        {
            var inputDroplet1 = agents[mergeCommand.InputName1];
            var inputDroplet2 = agents[mergeCommand.InputName2];

            bool areInPosition = inputDroplet1.PositionX == mergePositions.Item1.optimalX &&
                                 inputDroplet1.PositionY == mergePositions.Item1.optimalY &&
                                 inputDroplet2.PositionX == mergePositions.Item2.optimalX &&
                                 inputDroplet2.PositionY == mergePositions.Item2.optimalY;

            // If the droplets are already in position, return true
            if (areInPosition)
            {
                return true;
            }

            // Move inputDroplet1 to be next to the merge position
            if (inputDroplet1.PositionX != mergePositions.Item1.optimalX || inputDroplet1.PositionY != mergePositions.Item1.optimalY)
            {
                var moveCommand = new Move(inputDroplet1.DropletName, mergePositions.Item1.optimalX, mergePositions.Item1.optimalY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move dropletCommand added for droplet 1: {moveCommand}");
            }

            // Move inputDroplet2 to be next to the merge position
            if (inputDroplet2.PositionX != mergePositions.Item2.optimalX || inputDroplet2.PositionY != mergePositions.Item2.optimalY)
            {
                var moveCommand = new Move(inputDroplet2.DropletName, mergePositions.Item2.optimalX, mergePositions.Item2.optimalY);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move dropletCommand added for droplet 2: {moveCommand}");

            }

            return false;
        }

        public bool InPositionToSplit(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            ((int optimalX, int optimalY), (int optimalX, int optimalY)) splitPositions,
            Dictionary<string, Agent> agents)
        {

            if (agents.ContainsKey(splitCommand.OutputName1) && splitCommand.OutputName1 != splitCommand.InputName)
            {
                throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName1} already exists.");
            }

            if (agents.ContainsKey(splitCommand.OutputName2) && splitCommand.OutputName2 != splitCommand.InputName)
            {
                throw new InvalidOperationException($"Droplet with name {splitCommand.OutputName2} already exists.");
            }

            if (splitCommand.OutputName2 == splitCommand.OutputName1)
            {
                throw new InvalidOperationException($"Droplet with the same names can not be split.");
            }

            int splitPositionX = (splitPositions.Item1.optimalX + splitPositions.Item2.optimalX) / 2;
            int splitPositionY = (splitPositions.Item1.optimalY + splitPositions.Item2.optimalY) / 2;

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
