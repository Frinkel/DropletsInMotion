using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Application.Services
{
    public class ActionService : IActionService
    {

        private readonly MoveHandler _moveHandler;
        private readonly ITemplateService _templateService;
        private readonly IContaminationService _contaminationService;
        private readonly IStoreService _storeService;
        private readonly ICommandLifetimeService _commandLifetimeService;

        public ActionService(ITemplateService templateService, IContaminationService contaminationService, IStoreService storeService, ICommandLifetimeService commandLifetimeService)
        {
            _templateService = templateService;
            _contaminationService = contaminationService;
            _moveHandler = new MoveHandler(_templateService);
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
        }

        public List<BoardAction> Merge(Dictionary<string, Agent> agents, Merge mergeCommand, byte[,] contaminationMap, double time)
        {
            // Add logic for processing the Merge command
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

            mergeActions.AddRange(_templateService.ApplyTemplate("mergeHorizontal", newAgent, time));
            _contaminationService.ApplyContaminationMerge(newAgent, contaminationMap);
            _contaminationService.PrintContaminationState(contaminationMap);
            Console.WriteLine(outputDroplet);
            return mergeActions;
        }

        public List<BoardAction> SplitByVolume(Dictionary<string, Agent> agents, SplitByVolume splitCommand, byte[,] contaminationMap, double time, int direction)
        {
            // Retrieve the input droplet
            Droplet inputDroplet = agents[splitCommand.InputName]
                                   ?? throw new InvalidOperationException($"No droplet found with name {splitCommand.InputName}.");

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

            Droplet outputDroplet1, outputDroplet2;
            string templateName;

            // Handle splitting based on direction
            switch (direction)
            {
                case 1: // Horizontal split, output 1 on the left (x-1) and output 2 on the right (x+1)
                    outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX - 1,
                        inputDroplet.PositionY, inputDroplet.Volume - splitCommand.Volume);

                    outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX + 1,
                        inputDroplet.PositionY, splitCommand.Volume);

                    templateName = "splitHorizontal";
                    break;

                case 3: // Horizontal split, but output 1 on the right (x+1) and output 2 on the left (x-1)
                    outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX + 1,
                        inputDroplet.PositionY, inputDroplet.Volume - splitCommand.Volume);

                    outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX - 1,
                        inputDroplet.PositionY, splitCommand.Volume);

                    templateName = "splitHorizontal";
                    break;

                case 2: // Vertical split, output 1 above (y-1) and output 2 below (y+1)
                    outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX,
                        inputDroplet.PositionY - 1, inputDroplet.Volume - splitCommand.Volume);

                    outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX,
                        inputDroplet.PositionY + 1, splitCommand.Volume);

                    templateName = "splitVertical";
                    break;

                case 4: // Vertical split, output 1 below (y+1) and output 2 above (y-1)
                    outputDroplet1 = new Droplet(splitCommand.OutputName1, inputDroplet.PositionX,
                        inputDroplet.PositionY + 1, inputDroplet.Volume - splitCommand.Volume);

                    outputDroplet2 = new Droplet(splitCommand.OutputName2, inputDroplet.PositionX,
                        inputDroplet.PositionY - 1, splitCommand.Volume);

                    templateName = "splitVertical";
                    break;

                default:
                    throw new InvalidOperationException($"Invalid direction {direction}. Allowed values are 1, 2, 3, or 4.");
            }

            // Remove the input droplet and add the new droplets to the dictionary
            Agent newAgent1 = new Agent(outputDroplet1.DropletName, outputDroplet1.PositionX, outputDroplet1.PositionY, outputDroplet1.Volume, agents[inputDroplet.DropletName].SubstanceId);
            Agent newAgent2 = new Agent(outputDroplet2.DropletName, outputDroplet2.PositionX, outputDroplet2.PositionY, outputDroplet2.Volume, agents[inputDroplet.DropletName].SubstanceId);
            agents.Remove(inputDroplet.DropletName);

            agents[outputDroplet1.DropletName] = newAgent1;
            agents[outputDroplet2.DropletName] = newAgent2;
            _contaminationService.ApplyContamination(newAgent1, contaminationMap);
            _contaminationService.ApplyContamination(newAgent2, contaminationMap);

            // Apply the appropriate template based on direction
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

        public bool InPositionToMix(Mix mixCommand, Dictionary<string, Agent> agents, List<ICommand> movesToExecute)
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


        public bool InPositionToStore(Store storeCommand, Dictionary<string, Agent> agents, List<ICommand> movesToExecute)
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


    }
}
