using System.Formats.Asn1;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.UI;

namespace DropletsInMotion.Application.Services
{
    public class SchedulerService : ISchedulerService
    {
        //private IDropletCommand DropletCommand { get; set; }
        //private List<IDropletCommand> CommandList { get; set; }
        //private Dictionary<string, Droplet> Droplets { get; set; }

        IContaminationService _contaminationService;
        public SchedulerService(IContaminationService contaminationService)
        {
            _contaminationService = contaminationService;
        }

        public ScheduledPosition ScheduleCommand(IDropletCommand dropletCommand, Dictionary<string, Agent> agents, List<int>[,] contaminationMap, List<ITemplate> templates)
        {
            switch (dropletCommand)
            {
                case Merge mergeCommand:
                    Agent d1 = agents[mergeCommand.InputName1];
                    Agent d2 = agents[mergeCommand.InputName2];

                    int d1SubstanceId = agents[d1.DropletName].SubstanceId;
                    int d2SubstanceId = agents[d2.DropletName].SubstanceId;


                    var optimalPositions = FindOptimalPositions(mergeCommand.PositionX, mergeCommand.PositionY,
                        d1.PositionX, d1.PositionY, d2.PositionX, d2.PositionY, contaminationMap, d1SubstanceId, d2SubstanceId, agents, templates, mergeCommand);

                    return optimalPositions;
                    //return null;

                case SplitByVolume splitByVolumeCommand:

                    Agent dInput = agents[splitByVolumeCommand.InputName];
                    int dOutput1PositionX = splitByVolumeCommand.PositionX1;
                    int dOutput1PositionY = splitByVolumeCommand.PositionY1;

                    int dOutput2PositionX = splitByVolumeCommand.PositionX2;
                    int dOutput2PositionY = splitByVolumeCommand.PositionY2;


                    d1SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;
                    d2SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;


                    optimalPositions = FindOptimalPositions(dInput.PositionX, dInput.PositionY,
                        dOutput1PositionX, dOutput1PositionY, dOutput2PositionX, dOutput2PositionY, contaminationMap, d1SubstanceId, d2SubstanceId, agents, templates, splitByVolumeCommand);
                    return optimalPositions;
                    
                case SplitByRatio splitByRatioCommand:
                    // This is handles as split by volume
                    break;

                default:
                    throw new InvalidOperationException("Tried to schedule a non-schedulable dropletCommand!");
            }

            return null;
        }


        public ScheduledPosition FindOptimalPositions(int commandX, int commandY, int d1X, int d1Y, int d2X, int d2Y, List<int>[,] contaminationMap, 
                                                      int d1SubstanceId, int d2SubstanceId, Dictionary<string, Agent> agents, List<ITemplate> templates, IDropletCommand command)
        {
            int minBoundingX = Math.Min(commandX, Math.Min(d1X, d2X));
            int minBoundingY = Math.Min(commandY, Math.Min(d1Y, d2Y));
            int maxBoundingX = Math.Max(commandX, Math.Max(d1X, d2X));
            int maxBoundingY = Math.Max(commandY, Math.Max(d1Y, d2Y));


            int boardWidth = contaminationMap.GetLength(0);
            int boardHeight = contaminationMap.GetLength(1);



            List<(int x, int y)> allOtherAgentPositions = new List<(int x, int y)>();
            foreach (var otherAgent in agents.Values)
            {

                if (command.GetInputDroplets().Contains(otherAgent.DropletName)) continue;

                var otherAgentSnake = otherAgent.SnakeBody;
                allOtherAgentPositions.AddRange(otherAgentSnake);

                if (otherAgent.GetMaximumSnakeLength() > otherAgentSnake.Count)
                {
                    allOtherAgentPositions.AddRange(otherAgent.GetAllAgentPositions());
                }
            }


            int increment = 0;

            do {
                minBoundingX = Math.Clamp(minBoundingX - increment, 0, boardWidth - 1);
                minBoundingY = Math.Clamp(minBoundingY - increment, 0, boardHeight - 1);
                maxBoundingX = Math.Clamp(maxBoundingX + increment, 0, boardWidth - 1);
                maxBoundingY = Math.Clamp(maxBoundingY + increment, 0, boardHeight - 1);

                int bestScore = int.MaxValue;
                ScheduledPosition bestPositions = null;

                for (int x = minBoundingX; x <= maxBoundingX; x++)
                {
                    for (int y = minBoundingY; y <= maxBoundingY; y++)
                    {

                        if (_contaminationService.IsConflicting(contaminationMap, x, y, new List<int>(){d1SubstanceId, d2SubstanceId}))
                        {
                            continue;
                        }


                        int estimatedMinScore = Math.Abs(x - commandX) + Math.Abs(y - commandY);
                        if (estimatedMinScore >= bestScore)
                        {
                            continue;
                        }

                        var optimalPositions = FindOptimalDirections(x, y, d1X, d1Y, d2X, d2Y, templates, command);


                        // Out of bounds
                        if (optimalPositions.X1 < minBoundingX || optimalPositions.X1 > maxBoundingX ||
                            optimalPositions.Y1 < minBoundingY || optimalPositions.Y1 > maxBoundingY ||
                            optimalPositions.X2 < minBoundingX || optimalPositions.X2 > maxBoundingX ||
                            optimalPositions.Y2 < minBoundingY || optimalPositions.Y2 > maxBoundingY)
                        {
                            continue;
                        }


                        if (_contaminationService.IsConflicting(contaminationMap, optimalPositions.X1, optimalPositions.Y1, new List<int>() { d1SubstanceId, d2SubstanceId }))
                        {
                            continue;
                        }
                        if (_contaminationService.IsConflicting(contaminationMap, optimalPositions.X2, optimalPositions.Y2, new List<int>() { d1SubstanceId, d2SubstanceId }))
                        {
                            continue;
                        }

                        // Same position
                        if (optimalPositions.X1 == optimalPositions.X2 && optimalPositions.Y1 == optimalPositions.Y2)
                        {
                            continue;
                        }

                        int d1DistanceToOrigin = Math.Abs(optimalPositions.X1 - d1X) +
                                                 Math.Abs(optimalPositions.Y1 - d1Y);

                        int d2DistanceToOrigin = Math.Abs(optimalPositions.X2 - d2X) +
                                                 Math.Abs(optimalPositions.Y2 - d2Y);

                        int distanceToTarget = Math.Abs(optimalPositions.SingularX - commandX) +
                                               Math.Abs(optimalPositions.SingularY - commandY);





                        int totalScore = d1DistanceToOrigin + d2DistanceToOrigin + distanceToTarget;


                        bool d1CrossesD2 = d1X < d2X && optimalPositions.X1 > optimalPositions.X2 ||
                                           d1X > d2X && optimalPositions.X1 < optimalPositions.X2 ||
                                           d1Y < d2Y && optimalPositions.Y1 > optimalPositions.Y2 ||
                                           d1Y > d2Y && optimalPositions.Y1 < optimalPositions.Y2;

                        bool d2CrossesD1 = d2X < d1X && optimalPositions.X2 > optimalPositions.X1 ||
                                           d2X > d1X && optimalPositions.X2 < optimalPositions.X1 ||
                                           d2Y < d1Y && optimalPositions.Y2 > optimalPositions.Y1 ||
                                           d2Y > d1Y && optimalPositions.Y2 < optimalPositions.Y1;

                        if (d1CrossesD2 || d2CrossesD1)
                        {
                            totalScore += 5;
                        }

                        if (totalScore >= bestScore)
                        {
                            continue;
                        }


                        

                        // Out-of-bounds template check and applicable check
                        List<int> substanceIds = new List<int>() { d1SubstanceId, d2SubstanceId };
                        bool isTemplateApplicable = IsTemplateApplicable(optimalPositions, boardWidth, boardHeight, contaminationMap, substanceIds, allOtherAgentPositions);
                        

                        if (!isTemplateApplicable)
                        {
                            continue;
                        }

                        bestScore = totalScore; 
                        bestPositions = optimalPositions;
                    }
                }


                if (bestPositions != null)
                {
                    return bestPositions;
                }

                increment++;
            } while ((minBoundingX != 0 || maxBoundingX != boardWidth - 1) ||
                     (minBoundingY != 0 || maxBoundingY != boardHeight - 1));



            throw new CommandException($"There was no positions where command \"{command}\" could be applied!", command);
        }


        public bool IsTemplateApplicable(ScheduledPosition scheduledPosition, int boardWidth, int boardHeight, List<int>[,] contaminationMap, List<int> substanceIds, List<(int x, int )> allOtherAgentPositions)
        {
            var offsets = new List<(int xOffset, int yOffset)>
            {
                (0, 0),   // Original position
                (1, 0),   // Right
                (-1, 0),  // Left
                (0, 1),   // Down
                (0, -1),  // Up
                (1, -1),  // Bottom-right diagonal
                (-1, 1),  // Top-left diagonal
                (1, 1),   // Top-right diagonal
                (-1, -1)  // Bottom-left diagonal
            };

            foreach (var val in scheduledPosition.Template.Blocks)
            {
                foreach (var kvp in val)
                {
                    foreach (var pos in kvp.Value)
                    {
                        var relativeX = pos.x + scheduledPosition.OriginX;
                        var relativeY = pos.y + scheduledPosition.OriginY;

                        // Ensure the template is within bounds of the board
                        if ((relativeX < 0 || relativeX >= boardWidth) ||
                            (relativeY < 0 || relativeY >= boardHeight))
                        {
                            return false;
                        }


                        // Check contamination on active electrodes
                        List<int> contaminations = contaminationMap[relativeX, relativeY];

                        // Check to see if there are values in the contamination that are not in the substance ids
                        var anyIllegalSubstances = contaminations.Except(substanceIds).Any();

                        if (anyIllegalSubstances)
                        {
                            return false;
                        }

                        // Check to see if there is another agent too close to the template
                        foreach (var (xOffset, yOffset) in offsets)
                        {
                            var nRelativeX = Math.Clamp(relativeX + xOffset, 0, boardWidth - 1);
                            var nRelativeY = Math.Clamp(relativeY + yOffset, 0, boardHeight - 1);

                            if (allOtherAgentPositions.Contains((nRelativeX, nRelativeY))) return false;
                        }
                    }
                }
            }

            return true;
        }

        private ScheduledPosition FindOptimalDirections(int originX, int originY, int target1X, int target1Y,
            int target2X, int target2Y, List<ITemplate> templates, IDropletCommand command)
        {
            switch (command)
            {
                case Merge mergeCommand:
                    return FindOptimalDirections(originX, originY, target1X, target1Y, target2X, target2Y, templates, mergeCommand);

                case SplitByVolume splitCommand:
                    return FindOptimalDirections(originX, originY, target1X, target1Y, target2X, target2Y, templates, splitCommand);

                default:
                    throw new CommandException($"Scheduler does not know the command.", command);
            }
        }

        private ScheduledPosition FindOptimalDirections(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y, List<ITemplate> templates, Merge mergeCommand)
        {
            ITemplate chosenTemplate = null;
            int cost = Int32.MaxValue;
            int d1OptimalX = target1X;
            int d1OptimalY = target1Y;
            int d2OptimalX = target2X;
            int d2OptimalY = target2Y;

            foreach (var template in templates)
            {
                int d1XDiff = originX - target1X + template.FinalPositions.First().Value.x;
                int d1YDiff = originY - target1Y + template.FinalPositions.First().Value.y;
                int d2XDiff = originX - target2X + template.FinalPositions.First().Value.x;
                int d2YDiff = originY - target2Y + template.FinalPositions.First().Value.y;

                int totalCost = 0;

                totalCost += Math.Abs(d1XDiff + template.InitialPositions.First().Value.x) + Math.Abs(d1YDiff + template.InitialPositions.First().Value.y);
                totalCost += Math.Abs(d2XDiff + template.InitialPositions.Last().Value.x) + Math.Abs(d2YDiff + template.InitialPositions.Last().Value.y);

                if (totalCost < cost)
                {
                    cost = totalCost;
                    chosenTemplate = template;
                    d1OptimalX = originX + template.InitialPositions.First().Value.x;
                    d1OptimalY = originY + template.InitialPositions.First().Value.y;
                    d2OptimalX = originX + template.InitialPositions.Last().Value.x;
                    d2OptimalY = originY + template.InitialPositions.Last().Value.y;
                }


                totalCost = 0;

                totalCost += Math.Abs(d1XDiff + template.InitialPositions.Last().Value.x) + Math.Abs(d1YDiff + template.InitialPositions.Last().Value.y);
                totalCost += Math.Abs(d2XDiff + template.InitialPositions.First().Value.x) + Math.Abs(d2YDiff + template.InitialPositions.First().Value.y);


                if (totalCost < cost)
                {
                    cost = totalCost;
                    chosenTemplate = template;
                    d1OptimalX = originX + template.InitialPositions.Last().Value.x;
                    d1OptimalY = originY + template.InitialPositions.Last().Value.y;
                    d2OptimalX = originX + template.InitialPositions.First().Value.x;
                    d2OptimalY = originY + template.InitialPositions.First().Value.y;
                }

            }


            if (chosenTemplate == null)
            {
                throw new Exception("No template could be found for the position"); // TODO: REDO EXCEPTION
            }

            ScheduledPosition optimalPosition =
                new ScheduledPosition(chosenTemplate, d1OptimalX, d1OptimalY, d2OptimalX, d2OptimalY,
                    originX + chosenTemplate.FinalPositions.First().Value.x, originY + chosenTemplate.FinalPositions.First().Value.y, originX, originY);

            return optimalPosition;
        }

        private ScheduledPosition FindOptimalDirections(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y, List<ITemplate> templates, SplitByVolume splitByVolumeCommand)
        {
            ITemplate chosenTemplate = null;
            int cost = Int32.MaxValue;
            int d1OptimalX = target1X;
            int d1OptimalY = target1Y;
            int d2OptimalX = target2X;
            int d2OptimalY = target2Y;

            foreach (var template in templates)
            {
                int d1XDiff = originX - target1X + template.InitialPositions.First().Value.x;
                int d1YDiff = originY - target1Y + template.InitialPositions.First().Value.y;
                int d2XDiff = originX - target2X + template.InitialPositions.First().Value.x;
                int d2YDiff = originY - target2Y + template.InitialPositions.First().Value.y;


                int totalCost = 0;

                totalCost += Math.Abs(d1XDiff + template.FinalPositions[splitByVolumeCommand.OutputName1].x) + Math.Abs(d1YDiff + template.FinalPositions[splitByVolumeCommand.OutputName1].y);
                totalCost += Math.Abs(d2XDiff + template.FinalPositions[splitByVolumeCommand.OutputName2].x) + Math.Abs(d2YDiff + template.FinalPositions[splitByVolumeCommand.OutputName2].y);

                if (totalCost < cost)
                {
                    cost = totalCost;
                    chosenTemplate = template;
                    d1OptimalX = originX + template.FinalPositions[splitByVolumeCommand.OutputName1].x;
                    d1OptimalY = originY + template.FinalPositions[splitByVolumeCommand.OutputName1].y;
                    d2OptimalX = originX + template.FinalPositions[splitByVolumeCommand.OutputName2].x;
                    d2OptimalY = originY + template.FinalPositions[splitByVolumeCommand.OutputName2].y;
                }

            }

            if (chosenTemplate == null)
            {
                throw new Exception("No template was chosen");
            }

            ScheduledPosition optimalPosition =
                new ScheduledPosition(chosenTemplate, d1OptimalX, d1OptimalY, d2OptimalX, d2OptimalY,
                    originX, originY, originX, originY);

            return optimalPosition;
        }

        
    }
}
