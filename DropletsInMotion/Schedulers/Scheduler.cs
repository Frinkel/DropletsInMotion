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

        public ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY))? ScheduleCommand(List<ICommand> commandsToBeScheduled, Dictionary<string, Droplet> droplets,
            Dictionary<string, Agent> agents, byte[,] contaminationMap)
        {
            foreach (var command in commandsToBeScheduled)
            {
                switch (command)
                {
                    case Merge mergeCommand:
                        Droplet d1 = droplets[mergeCommand.InputName1];
                        Droplet d2 = droplets[mergeCommand.InputName2];

                        byte d1SubstanceId = agents[d1.DropletName].SubstanceId;
                        byte d2SubstanceId = agents[d2.DropletName].SubstanceId;

                        byte? subtanceId = d1SubstanceId == d2SubstanceId ? d1SubstanceId : null;
                        Console.WriteLine($"SubstanceId {subtanceId}");

                        var optimalPositions = FindOptimalPositions(mergeCommand.PositionX, mergeCommand.PositionY,
                            d1.PositionX, d1.PositionY, d2.PositionX, d2.PositionY, contaminationMap, subtanceId);
                        return optimalPositions;

                    case SplitByVolume splitByVolumeCommand:
                        // Implement logic for SplitByVolume if needed
                        break;

                    case SplitByRatio splitByRatioCommand:
                        // Implement logic for SplitByRatio if needed
                        break;

                    default:
                        throw new InvalidOperationException("Tried to schedule a non-schedulable command!");
                }
            }

            return null;
        }


        public ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY))? FindOptimalPositions(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y, byte[,] contaminationMap, byte? substanceId = null)
        {
            int minBoundingX = Math.Min(originX, Math.Min(target1X, target2X));
            int minBoundingY = Math.Min(originY, Math.Min(target1Y, target2Y));
            int maxBoundingX = Math.Max(originX, Math.Max(target1X, target2X));
            int maxBoundingY = Math.Max(originY, Math.Max(target1Y, target2Y));

            Console.WriteLine($"Bounding box: {((minBoundingX, minBoundingY), (maxBoundingX, maxBoundingY))}");

            int bestScore = int.MaxValue;
            ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) bestPositions = ((0, 0), (0, 0));

            for (int x = minBoundingX; x <= maxBoundingX; x++)
            {
                for (int y = minBoundingY; y <= maxBoundingY; y++)
                {
                    if (contaminationMap[x, y] == 255)
                    {
                        continue;
                    }

                    if ((substanceId == null && contaminationMap[x, y] != 0) || (substanceId != null &&
                            (contaminationMap[x, y] != 0 || contaminationMap[x, y] != substanceId)))
                    {
                        continue;
                    }


                    int estimatedMinScore = Math.Abs(x - originX) + Math.Abs(y - originY);
                    if (estimatedMinScore >= bestScore)
                    {
                        continue;
                    }

                    var optimalPositions = FindOptimalDirections(x, y, target1X, target1Y, target2X, target2Y);


                    // Out of bounds
                    if (optimalPositions.Item1.d1OptimalX < minBoundingX || optimalPositions.Item1.d1OptimalX > maxBoundingX ||
                        optimalPositions.Item1.d1OptimalY < minBoundingY || optimalPositions.Item1.d1OptimalY > maxBoundingY ||
                        optimalPositions.Item2.d2OptimalX < minBoundingX || optimalPositions.Item2.d2OptimalX > maxBoundingX ||
                        optimalPositions.Item2.d2OptimalY < minBoundingY || optimalPositions.Item2.d2OptimalY > maxBoundingY)
                    {
                        continue;
                    }


                    if ((substanceId == null && contaminationMap[optimalPositions.Item1.d1OptimalX, optimalPositions.Item1.d1OptimalY] != 0) || (substanceId != null &&
                            (contaminationMap[optimalPositions.Item1.d1OptimalX, optimalPositions.Item1.d1OptimalY] != 0 || contaminationMap[optimalPositions.Item1.d1OptimalX, optimalPositions.Item1.d1OptimalY] != substanceId)))
                    {
                        continue;
                    }

                    if ((substanceId == null && contaminationMap[optimalPositions.Item2.d2OptimalX, optimalPositions.Item2.d2OptimalY] != 0) || (substanceId != null &&
                            (contaminationMap[optimalPositions.Item2.d2OptimalX, optimalPositions.Item2.d2OptimalY] != 0 || contaminationMap[optimalPositions.Item2.d2OptimalX, optimalPositions.Item2.d2OptimalY] != substanceId)))
                    {
                        continue;
                    }

                    // Same position
                    if (optimalPositions.Item1 == optimalPositions.Item2)
                    {
                        continue;
                    }

                    int d1DistanceToOrigin = Math.Abs(optimalPositions.Item1.d1OptimalX - target1X) +
                                             Math.Abs(optimalPositions.Item1.d1OptimalY - target1Y);

                    int d2DistanceToOrigin = Math.Abs(optimalPositions.Item2.d2OptimalX - target2X) +
                                             Math.Abs(optimalPositions.Item2.d2OptimalY - target2Y);

                    int distanceToTarget = Math.Abs(x - originX) +
                                           Math.Abs(y- originY);





                    int totalScore = d1DistanceToOrigin + d2DistanceToOrigin + distanceToTarget;


                    bool d1CrossesD2 = (target1X < target2X && optimalPositions.Item1.d1OptimalX > optimalPositions.Item2.d2OptimalX) ||
                                       (target1X > target2X && optimalPositions.Item1.d1OptimalX < optimalPositions.Item2.d2OptimalX) ||
                                       (target1Y < target2Y && optimalPositions.Item1.d1OptimalY > optimalPositions.Item2.d2OptimalY) ||
                                       (target1Y > target2Y && optimalPositions.Item1.d1OptimalY < optimalPositions.Item2.d2OptimalY);

                    bool d2CrossesD1 = (target2X < target1X && optimalPositions.Item2.d2OptimalX > optimalPositions.Item1.d1OptimalX) ||
                                       (target2X > target1X && optimalPositions.Item2.d2OptimalX < optimalPositions.Item1.d1OptimalX) ||
                                       (target2Y < target1Y && optimalPositions.Item2.d2OptimalY > optimalPositions.Item1.d1OptimalY) ||
                                       (target2Y > target1Y && optimalPositions.Item2.d2OptimalY < optimalPositions.Item1.d1OptimalY);

                    if (d1CrossesD2 || d2CrossesD1)
                    {
                        totalScore += 5;
                    }

                    if (totalScore >= bestScore)
                    {
                        continue;
                    }

                    Console.WriteLine($"Position {(x, y)} with droplets at {optimalPositions} has a score of {totalScore}");
                    
                    bestScore = totalScore;
                    bestPositions = optimalPositions;
                }
            }

            Console.WriteLine($"Optimal merge positions: Droplet 1 at ({bestPositions.Item1.d1OptimalX}, {bestPositions.Item1.d1OptimalY}), " +
                              $"Droplet 2 at ({bestPositions.Item2.d2OptimalX}, {bestPositions.Item2.d2OptimalY}) with a score of {bestScore}");
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
