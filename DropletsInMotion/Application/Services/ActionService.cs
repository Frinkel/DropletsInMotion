using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Communication;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Communication.Services;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Presentation.Services;

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
        private readonly ITemplateRepository _templateRepository;
        private readonly IPlatformService _platformService;


        public ActionService(ITemplateService templateService, IContaminationService contaminationService, IStoreService storeService, 
                             ICommandLifetimeService commandLifetimeService, IDeviceRepository deviceRepository, ITemplateRepository templateRepository,
                             IPlatformService platformService)
        {
            _templateService = templateService;
            _contaminationService = contaminationService;
            _moveHandler = new MoveHandler(_templateService);
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
            _deviceRepository = deviceRepository;
            _templateRepository = templateRepository;
            _platformService = platformService;
        }

        public List<BoardAction> Merge(Dictionary<string, Agent> agents, Merge mergeCommand, byte[,] contaminationMap, double time, ScheduledPosition mergePositions)
        {
            // Add logic for processing the Merge dropletCommand

            //Merge
            Droplet inputDroplet1 = agents[mergeCommand.InputName1]
                                    ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName1}.");

            Droplet inputDroplet2 = agents[mergeCommand.InputName2]
                                    ?? throw new InvalidOperationException($"No droplet found with name {mergeCommand.InputName2}.");


            //List<BoardAction> mergeActions = new List<BoardAction>();

            int outPutDropletX = mergePositions.CommandX;
            int outPutDropletY = mergePositions.CommandY;
            Droplet outputDroplet = new Droplet(mergeCommand.OutputName, outPutDropletX, outPutDropletY,
                inputDroplet1.Volume + inputDroplet2.Volume);


            List<BoardAction> mergeActions = mergePositions.Template.Apply(_platformService.Board[outputDroplet.PositionX][outputDroplet.PositionY].Id, time, 1);

            Agent inputAgent1 = agents[inputDroplet1.DropletName];
            Agent inputAgent2 = agents[inputDroplet2.DropletName];

            Agent newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume);

            if (inputAgent1.SubstanceId == inputAgent2.SubstanceId)
            {
                newAgent = new Agent(outputDroplet.DropletName, outPutDropletX, outPutDropletY, outputDroplet.Volume, inputAgent1.SubstanceId);
            }

            agents.Remove(inputDroplet1.DropletName);
            agents.Remove(inputDroplet2.DropletName);

            agents.Add(newAgent.DropletName, newAgent);

            //_contaminationService.ApplyContaminationMerge(newAgent, contaminationMap);

            _contaminationService.ApplyContaminationMerge(inputAgent1, inputAgent2, newAgent, mergePositions, contaminationMap);

            //mergePositions.Template.ContaminationPositions.ForEach(pos => _contaminationService.ApplyIfInBoundsMerge(contaminationMap, newAgent.PositionX + pos.x, 
            //    newAgent.PositionY + pos.y, mergePositions.Template.FinalPositions.First().Value.x, mergePositions.Template.FinalPositions.First().Value.y,
            //    inputAgent1.SubstanceId, inputAgent2.SubstanceId, newAgent.SubstanceId));
            //mergePositions.Template.ContaminationPositions.ForEach(pos => Console.WriteLine($"{newAgent.PositionX + pos.x}, {newAgent.PositionY + pos.y}"));

            _contaminationService.PrintContaminationState(contaminationMap);
            Console.WriteLine(outputDroplet);
            return mergeActions;
        }

        public List<BoardAction> SplitByVolume(
             Dictionary<string, Agent> agents,
             SplitByVolume splitCommand,
             byte[,] contaminationMap,
             double time,
             ScheduledPosition splitPositions)
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


            bool isBetweenHorizontally = (inputDroplet.PositionX == (splitPositions.X1 + splitPositions.X2) / 2) &&
                                         (inputDroplet.PositionY == splitPositions.Y1 && inputDroplet.PositionY == splitPositions.Y2);

            bool isBetweenVertically = (inputDroplet.PositionY == (splitPositions.Y1 + splitPositions.Y2) / 2) &&
                                       (inputDroplet.PositionX == splitPositions.X1 && inputDroplet.PositionX == splitPositions.X2);

            if (!isBetweenHorizontally && !isBetweenVertically)
            {
                throw new InvalidOperationException("Input droplet is not positioned between the specified split positions.");
            }



            Droplet outputDroplet1 = new Droplet(splitCommand.OutputName1, splitPositions.X1, splitPositions.Y1, inputDroplet.Volume - splitCommand.Volume);
            Droplet outputDroplet2 = new Droplet(splitCommand.OutputName2, splitPositions.X2, splitPositions.Y2, splitCommand.Volume);
            

            Agent newAgent1 = new Agent(outputDroplet1.DropletName, outputDroplet1.PositionX, outputDroplet1.PositionY, outputDroplet1.Volume, agents[inputDroplet.DropletName].SubstanceId);
            Agent newAgent2 = new Agent(outputDroplet2.DropletName, outputDroplet2.PositionX, outputDroplet2.PositionY, outputDroplet2.Volume, agents[inputDroplet.DropletName].SubstanceId);
            
            agents.Remove(inputDroplet.DropletName);

            agents[outputDroplet1.DropletName] = newAgent1;
            agents[outputDroplet2.DropletName] = newAgent2;
            //_contaminationService.ApplyContamination(newAgent1, contaminationMap);
            //_contaminationService.ApplyContamination(newAgent2, contaminationMap);

            splitPositions.Template.ContaminationPositions.ForEach(pos => _contaminationService.ApplyIfInBoundsWithContamination(contaminationMap, inputDroplet.PositionX + pos.x, inputDroplet.PositionY + pos.y, newAgent1.SubstanceId));
            splitPositions.Template.ContaminationPositions.ForEach(pos => Console.WriteLine($"{inputDroplet.PositionX + pos.x}, {inputDroplet.PositionY + pos.y}"));
            
            //_contaminationService.PrintContaminationState(contaminationMap);

            List<BoardAction> splitActions = splitPositions.Template.Apply(_platformService.Board[inputDroplet.PositionX][inputDroplet.PositionY].Id, time, 1);

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
                Console.WriteLine($"Move dropletCommand added for droplet 1: {moveCommand}");
            }

            // Move inputDroplet2 to be next to the merge position
            if (inputDroplet2.PositionX != mergePositions.X2 || inputDroplet2.PositionY != mergePositions.Y2)
            {
                var moveCommand = new Move(inputDroplet2.DropletName, mergePositions.X2, mergePositions.Y2);
                movesToExecute.Add(moveCommand);
                Console.WriteLine($"Move dropletCommand added for droplet 2: {moveCommand}");

            }

            return false;
        }

        public bool InPositionToSplit(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            ScheduledPosition splitPositions,
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
