using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services
{
    public class SchedulerService : ISchedulerService
    {
        //private IDropletCommand DropletCommand { get; set; }
        //private List<IDropletCommand> CommandList { get; set; }
        //private Dictionary<string, Droplet> Droplets { get; set; }


        enum Directions
        {
            LeftRight,
            RightLeft,
            UpDown,
            DownUp
        }

        public SchedulerService()
        {

        }

        public ScheduledPosition ScheduleCommand(IDropletCommand dropletCommand, Dictionary<string, Agent> agents, byte[,] contaminationMap, List<ITemplate> templates)
        {
            switch (dropletCommand)
            {
                case Merge mergeCommand:
                    Agent d1 = agents[mergeCommand.InputName1];
                    Agent d2 = agents[mergeCommand.InputName2];

                    byte d1SubstanceId = agents[d1.DropletName].SubstanceId;
                    byte d2SubstanceId = agents[d2.DropletName].SubstanceId;



                    Console.WriteLine($"a1 {d1}");
                    Console.WriteLine($"a2 {d2}");

                    var optimalPositions = FindOptimalPositions(mergeCommand.PositionX, mergeCommand.PositionY,
                        d1.PositionX, d1.PositionY, d2.PositionX, d2.PositionY, contaminationMap, d1SubstanceId, d2SubstanceId, templates, mergeCommand);

                    return optimalPositions;
                    //return null;

                case SplitByVolume splitByVolumeCommand:
                    // Implement logic for SplitByVolume if needed

                    Agent dInput = agents[splitByVolumeCommand.InputName];
                    int dOutput1PositionX = splitByVolumeCommand.PositionX1;
                    int dOutput1PositionY = splitByVolumeCommand.PositionY1;

                    int dOutput2PositionX = splitByVolumeCommand.PositionX2;
                    int dOutput2PositionY = splitByVolumeCommand.PositionY2;


                    d1SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;
                    d2SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;


                    optimalPositions = FindOptimalPositions(dInput.PositionX, dInput.PositionY,
                        dOutput1PositionX, dOutput1PositionY, dOutput2PositionX, dOutput2PositionY, contaminationMap, d1SubstanceId, d2SubstanceId, templates, splitByVolumeCommand);
                    return optimalPositions;
                    
                case SplitByRatio splitByRatioCommand:
                    // This is handles as split by volume
                    break;

                default:
                    throw new InvalidOperationException("Tried to schedule a non-schedulable dropletCommand!");
            }

            return null;
        }


        public ScheduledPosition FindOptimalPositions(int commandX, int commandY, int d1X, int d1Y, int d2X, int d2Y, byte[,] contaminationMap, 
                                                      byte d1SubstanceId, byte d2SubstanceId, List<ITemplate> templates, IDropletCommand command)
        {
            int minBoundingX = Math.Min(commandX, Math.Min(d1X, d2X));
            int minBoundingY = Math.Min(commandY, Math.Min(d1Y, d2Y));
            int maxBoundingX = Math.Max(commandX, Math.Max(d1X, d2X));
            int maxBoundingY = Math.Max(commandY, Math.Max(d1Y, d2Y));

            Console.WriteLine($"Bounding box: {((minBoundingX, minBoundingY), (maxBoundingX, maxBoundingY))}");

            int bestScore = int.MaxValue;
            ScheduledPosition bestPositions = null;

            for (int x = minBoundingX; x <= maxBoundingX; x++)
            {
                for (int y = minBoundingY; y <= maxBoundingY; y++)
                {
                    byte contamination = contaminationMap[x, y];
                    if (contamination != 0 && contamination != 255 &&
                        contamination != d1SubstanceId && contamination != d2SubstanceId)
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

                    byte d1OptimalPositionContamination = contaminationMap[optimalPositions.X1,
                        optimalPositions.Y1];
                    byte d2OptimalPositionContamination = contaminationMap[optimalPositions.X2,
                        optimalPositions.Y2];
                    if (d1OptimalPositionContamination == 255 || d1OptimalPositionContamination != 0 &&
                        d1OptimalPositionContamination != d1SubstanceId)
                    {
                        continue;
                    }
                    if (d2OptimalPositionContamination == 255 || d2OptimalPositionContamination != 0 &&
                        d2OptimalPositionContamination != d2SubstanceId)
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

                    int distanceToTarget = Math.Abs(x - commandX) +
                                           Math.Abs(y - commandY);





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

                    bestScore = totalScore;
                    bestPositions = optimalPositions;
                }
            }

            if (bestPositions == null)
            {
                throw new Exception("No optimal position found");
            }

            Console.WriteLine($"Optimal positions at ({bestPositions.X1}, {bestPositions.Y1}), " +
                              $"and ({bestPositions.X2}, {bestPositions.Y2}) with a score of {bestScore}");
            //throw new Exception("");
            

            return bestPositions;
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
                    throw new Exception("No schedular case for command");
            }
        }

        private ScheduledPosition FindOptimalDirections(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y, List<ITemplate> templates, Merge mergeCommand)
        {
            int d1XDiff = originX - target1X;
            int d1YDiff = originY - target1Y;
            int d2XDiff = originX - target2X;
            int d2YDiff = originY - target2Y;

            ITemplate chosenTemplate = null;
            int cost = Int32.MaxValue;
            int d1OptimalX = target1X;
            int d1OptimalY = target1Y;
            int d2OptimalX = target2X;
            int d2OptimalY = target2Y;

            foreach (var template in templates)
            {
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

                totalCost += Math.Abs(d1XDiff + template.FinalPositions.Last().Value.x) + Math.Abs(d1YDiff + template.FinalPositions.Last().Value.y);
                totalCost += Math.Abs(d2XDiff + template.FinalPositions.First().Value.x) + Math.Abs(d2YDiff + template.FinalPositions.First().Value.y);


                if (totalCost < cost)
                {
                    cost = totalCost;
                    chosenTemplate = template;
                    d1OptimalX = originX + template.FinalPositions.Last().Value.x;
                    d1OptimalY = originY + template.FinalPositions.Last().Value.y;
                    d2OptimalX = originX + template.FinalPositions.First().Value.x;
                    d2OptimalY = originY + template.FinalPositions.First().Value.y;
                }

            }






            //int leftRightDistance = Math.Abs(d1XDiff - 1) + Math.Abs(d1YDiff) +
            //                        Math.Abs(d2XDiff + 1) + Math.Abs(d2YDiff);

            //int rightLeftDistance = Math.Abs(d1XDiff + 1) + Math.Abs(d1YDiff) +
            //                        Math.Abs(d2XDiff - 1) + Math.Abs(d2YDiff);

            //int upDownDistance = Math.Abs(d1XDiff) + Math.Abs(d1YDiff - 1) +
            //                     Math.Abs(d2XDiff) + Math.Abs(d2YDiff + 1);

            //int downUpDistance = Math.Abs(d1XDiff) + Math.Abs(d1YDiff + 1) +
            //                     Math.Abs(d2XDiff) + Math.Abs(d2YDiff - 1);

            //var directionDeltas = new List<(int Distance, Directions Direction, int d1DeltaX, int d1DeltaY, int d2DeltaX, int d2DeltaY)>
            //{
            //    (leftRightDistance, Directions.LeftRight, -1, 0, +1, 0),
            //    (rightLeftDistance, Directions.RightLeft, +1, 0, -1, 0),
            //    (upDownDistance, Directions.UpDown, 0, -1, 0, +1),
            //    (downUpDistance, Directions.DownUp, 0, +1, 0, -1)
            //};

            //var optimal = directionDeltas.MinBy(tuple => tuple.Distance);

            //int d1OptimalX = originX + optimal.d1DeltaX;
            //int d1OptimalY = originY + optimal.d1DeltaY;
            //int d2OptimalX = originX + optimal.d2DeltaX;
            //int d2OptimalY = originY + optimal.d2DeltaY;
            //Console.WriteLine((originX, originY));
            //Console.WriteLine(((d1OptimalX, d1OptimalY), (d2OptimalX, d2OptimalY)));
            //throw new Exception("test");

            if (chosenTemplate == null)
            {
                throw new Exception("No template was chosen");
            }

            ScheduledPosition optimalPosition =
                new ScheduledPosition(chosenTemplate, d1OptimalX, d1OptimalY, d2OptimalX, d2OptimalY, originX, originY);

            return optimalPosition;
        }

        private ScheduledPosition FindOptimalDirections(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y, List<ITemplate> templates, SplitByVolume splitByVolumeCommand)
        {
            int d1XDiff = originX - target1X;
            int d1YDiff = originY - target1Y;
            int d2XDiff = originX - target2X;
            int d2YDiff = originY - target2Y;

            ITemplate chosenTemplate = null;
            int cost = Int32.MaxValue;
            int d1OptimalX = target1X;
            int d1OptimalY = target1Y;
            int d2OptimalX = target2X;
            int d2OptimalY = target2Y;

            foreach (var template in templates)
            {
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
                new ScheduledPosition(chosenTemplate, d1OptimalX, d1OptimalY, d2OptimalX, d2OptimalY, originX, originY);

            return optimalPosition;
        }

        
    }
}
