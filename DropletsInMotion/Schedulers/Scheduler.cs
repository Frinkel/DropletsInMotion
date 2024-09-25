using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Schedulers
{
    public class Scheduler
    {
        //private ICommand Command { get; set; }
        //private List<ICommand> CommandList { get; set; }
        //private Dictionary<string, Droplet> Droplets { get; set; }


        enum Directions
        {
            LeftRight,
            RightLeft,
            UpDown,
            DownUp
        }

        public Scheduler()
        {

        }

        public ((int optimalX, int optimalY), (int optimalX, int optimalY))? ScheduleCommand(ICommand command, Dictionary<string, Droplet> droplets,
            Dictionary<string, Agent> agents, byte[,] contaminationMap)
        {
                switch (command)
                {
                    case Merge mergeCommand:
                        Droplet d1 = droplets[mergeCommand.InputName1];
                        Droplet d2 = droplets[mergeCommand.InputName2];

                        byte d1SubstanceId = agents[d1.DropletName].SubstanceId;
                        byte d2SubstanceId = agents[d2.DropletName].SubstanceId;

                        var optimalPositions = FindOptimalPositions(mergeCommand.PositionX, mergeCommand.PositionY,
                            d1.PositionX, d1.PositionY, d2.PositionX, d2.PositionY, contaminationMap, d1SubstanceId, d2SubstanceId);
                        return optimalPositions;

                    case SplitByVolume splitByVolumeCommand:
                        // Implement logic for SplitByVolume if needed

                        Droplet dInput = droplets[splitByVolumeCommand.InputName];
                        int dOutput1PositionX = splitByVolumeCommand.PositionX1;
                        int dOutput1PositionY = splitByVolumeCommand.PositionY1;

                        int dOutput2PositionX = splitByVolumeCommand.PositionX2;
                        int dOutput2PositionY = splitByVolumeCommand.PositionY2;


                        d1SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;
                        d2SubstanceId = agents[splitByVolumeCommand.InputName].SubstanceId;


                        optimalPositions = FindOptimalPositions(dInput.PositionX, dInput.PositionY,
                            dOutput1PositionX, dOutput1PositionY, dOutput2PositionX, dOutput2PositionY, contaminationMap, d1SubstanceId, d2SubstanceId);
                        return optimalPositions;
                        /*
                         Droplet dInput = droplets[splitByVolumeCommand.InputName];

                           int splitPosX = dInput.PositionX;
                           int splitPosY = dInput.PositionY;

                           int dOutput1PositionX = splitByVolumeCommand.PositionX1;
                           int dOutput1PositionY = splitByVolumeCommand.PositionY1;

                           int dOutput2PositionX = splitByVolumeCommand.PositionX2;
                           int dOutput2PositionY = splitByVolumeCommand.PositionY2;

                           var optimalPositions = FindOptimalPositions(splitPosX, splitPosY, dOutput1PositionX, dOutput1PositionY, dOutput2PositionX,
                               dOutput2PositionY);

                           return optimalPositions;
                         */
                        break;

                    case SplitByRatio splitByRatioCommand:
                        // Implement logic for SplitByRatio if needed
                        break;

                    default:
                        throw new InvalidOperationException("Tried to schedule a non-schedulable command!");
                }

            return null;
        }


        public ((int optimalX, int optimalY), (int optimalX, int optimalY))? FindOptimalPositions(int commandX, int commandY, int d1X, int d1Y, int d2X, int d2Y, byte[,] contaminationMap, byte d1SubstanceId, byte d2SubstanceId)
        {
            int minBoundingX = Math.Min(commandX, Math.Min(d1X, d2X));
            int minBoundingY = Math.Min(commandY, Math.Min(d1Y, d2Y));
            int maxBoundingX = Math.Max(commandX, Math.Max(d1X, d2X));
            int maxBoundingY = Math.Max(commandY, Math.Max(d1Y, d2Y));

            Console.WriteLine($"Bounding box: {((minBoundingX, minBoundingY), (maxBoundingX, maxBoundingY))}");

            int bestScore = int.MaxValue;
            ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) bestPositions = ((0, 0), (0, 0));

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

                    var optimalPositions = FindOptimalDirections(x, y, d1X, d1Y, d2X, d2Y);


                    // Out of bounds
                    if (optimalPositions.Item1.d1OptimalX < minBoundingX || optimalPositions.Item1.d1OptimalX > maxBoundingX ||
                        optimalPositions.Item1.d1OptimalY < minBoundingY || optimalPositions.Item1.d1OptimalY > maxBoundingY ||
                        optimalPositions.Item2.d2OptimalX < minBoundingX || optimalPositions.Item2.d2OptimalX > maxBoundingX ||
                        optimalPositions.Item2.d2OptimalY < minBoundingY || optimalPositions.Item2.d2OptimalY > maxBoundingY)
                    {
                        continue;
                    }

                    byte d1OptimalPositionContamination = contaminationMap[optimalPositions.Item1.d1OptimalX,
                        optimalPositions.Item1.d1OptimalY];
                    byte d2OptimalPositionContamination = contaminationMap[optimalPositions.Item2.d2OptimalX,
                        optimalPositions.Item2.d2OptimalY];
                    if (d1OptimalPositionContamination == 255 || (d1OptimalPositionContamination != 0 &&
                        d1OptimalPositionContamination != d1SubstanceId))
                    {
                        continue;
                    }
                    if (d2OptimalPositionContamination == 255 || (d2OptimalPositionContamination != 0 &&
                        d2OptimalPositionContamination != d2SubstanceId))
                    {
                        continue;
                    }

                    // Same position
                    if (optimalPositions.Item1 == optimalPositions.Item2)
                    {
                        continue;
                    }

                    int d1DistanceToOrigin = Math.Abs(optimalPositions.Item1.d1OptimalX - d1X) +
                                             Math.Abs(optimalPositions.Item1.d1OptimalY - d1Y);

                    int d2DistanceToOrigin = Math.Abs(optimalPositions.Item2.d2OptimalX - d2X) +
                                             Math.Abs(optimalPositions.Item2.d2OptimalY - d2Y);

                    int distanceToTarget = Math.Abs(x - commandX) +
                                           Math.Abs(y- commandY);





                    int totalScore = d1DistanceToOrigin + d2DistanceToOrigin + distanceToTarget;


                    bool d1CrossesD2 = (d1X < d2X && optimalPositions.Item1.d1OptimalX > optimalPositions.Item2.d2OptimalX) ||
                                       (d1X > d2X && optimalPositions.Item1.d1OptimalX < optimalPositions.Item2.d2OptimalX) ||
                                       (d1Y < d2Y && optimalPositions.Item1.d1OptimalY > optimalPositions.Item2.d2OptimalY) ||
                                       (d1Y > d2Y && optimalPositions.Item1.d1OptimalY < optimalPositions.Item2.d2OptimalY);

                    bool d2CrossesD1 = (d2X < d1X && optimalPositions.Item2.d2OptimalX > optimalPositions.Item1.d1OptimalX) ||
                                       (d2X > d1X && optimalPositions.Item2.d2OptimalX < optimalPositions.Item1.d1OptimalX) ||
                                       (d2Y < d1Y && optimalPositions.Item2.d2OptimalY > optimalPositions.Item1.d1OptimalY) ||
                                       (d2Y > d1Y && optimalPositions.Item2.d2OptimalY < optimalPositions.Item1.d1OptimalY);

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

            Console.WriteLine($"Optimal positions at ({bestPositions.Item1.d1OptimalX}, {bestPositions.Item1.d1OptimalY}), " +
                              $"and ({bestPositions.Item2.d2OptimalX}, {bestPositions.Item2.d2OptimalY}) with a score of {bestScore}");

            return bestPositions;
        }

        private ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) FindOptimalDirections(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y)
        {

            int d1XDiff = originX - target1X;
            int d1YDiff = originY - target1Y;
            int d2XDiff = originX - target2X;
            int d2YDiff = originY - target2Y;

            int leftRightDistance = Math.Abs(d1XDiff - 1) + Math.Abs(d1YDiff) +
                                    Math.Abs(d2XDiff + 1) + Math.Abs(d2YDiff);

            int rightLeftDistance = Math.Abs(d1XDiff + 1) + Math.Abs(d1YDiff) +
                                    Math.Abs(d2XDiff - 1) + Math.Abs(d2YDiff);

            int upDownDistance = Math.Abs(d1XDiff) + Math.Abs(d1YDiff - 1) +
                                 Math.Abs(d2XDiff) + Math.Abs(d2YDiff + 1);

            int downUpDistance = Math.Abs(d1XDiff) + Math.Abs(d1YDiff + 1) +
                                 Math.Abs(d2XDiff) + Math.Abs(d2YDiff - 1);

            var directionDeltas = new List<(int Distance, Directions Direction, int d1DeltaX, int d1DeltaY, int d2DeltaX, int d2DeltaY)>
            {
                (leftRightDistance, Directions.LeftRight, -1, 0, +1, 0),
                (rightLeftDistance, Directions.RightLeft, +1, 0, -1, 0),
                (upDownDistance, Directions.UpDown, 0, -1, 0, +1),
                (downUpDistance, Directions.DownUp, 0, +1, 0, -1)
            };

            var optimal = directionDeltas.MinBy(tuple => tuple.Distance);

            int d1OptimalX = originX + optimal.d1DeltaX;
            int d1OptimalY = originY + optimal.d1DeltaY;
            int d2OptimalX = originX + optimal.d2DeltaX;
            int d2OptimalY = originY + optimal.d2DeltaY;

            return ((d1OptimalX, d1OptimalY), (d2OptimalX, d2OptimalY));
        }

    }
}
