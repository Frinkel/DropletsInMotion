using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Domain;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Routers
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

        public ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) ScheduleCommand(ICommand command,
            Dictionary<string, Droplet> droplets)
        {
            switch (command)
            {
                case (Merge mergeCommand):
                    return ScheduleMergePosition(mergeCommand, droplets);
                case (SplitByVolume splitByVolumeCommand):
                    return ScheduleSplitPosition(splitByVolumeCommand, droplets);
                case (SplitByRatio splitByRatioCommand):
                    return ScheduleSplitPosition(splitByRatioCommand, droplets);
                default:
                    throw new InvalidOperationException("We tried to schedule a non-schduleable command!");
            }
        }

        private ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) FindOptimalPositions(int originX, int originY, int target1X, int target1Y, int target2X, int target2Y)
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

        public ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) ScheduleMergePosition(ICommand command, Dictionary<string, Droplet> droplets)
        {
            if (command is Merge mergeCommand)
            {
                // Get the input droplets
                Droplet d1 = droplets[mergeCommand.InputName1];
                Droplet d2 = droplets[mergeCommand.InputName2];

                int d1PositionX = d1.PositionX;
                int d1PositionY = d1.PositionY;

                int d2PositionX = d2.PositionX;
                int d2PositionY = d2.PositionY;

                int mergePosX = mergeCommand.PositionX;
                int mergePosY = mergeCommand.PositionY;

                var optimalPositions = FindOptimalPositions(mergePosX, mergePosY, d1PositionX, d1PositionY, d2PositionX,
                    d2PositionY);

                return optimalPositions;

            }
            else
            {
                throw new InvalidOperationException("Tried to schedule a merge for a non-merge command!");
            }
        }


        public ((int d1OptimalX, int d1OptimalY), (int d2OptimalX, int d2OptimalY)) ScheduleSplitPosition(ICommand command, Dictionary<string, Droplet> droplets)
        {
            if (command is SplitByVolume splitByVolumeCommand)
            {

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
            } else if (command is SplitByRatio splitByRatioCommand)
            {

                Droplet dInput = droplets[splitByRatioCommand.InputName];

                int splitPosX = dInput.PositionX;
                int splitPosY = dInput.PositionY;

                int dOutput1PositionX = splitByRatioCommand.PositionX1;
                int dOutput1PositionY = splitByRatioCommand.PositionY1;

                int dOutput2PositionX = splitByRatioCommand.PositionX2;
                int dOutput2PositionY = splitByRatioCommand.PositionY2;

                var optimalPositions = FindOptimalPositions(splitPosX, splitPosY, dOutput1PositionX, dOutput1PositionY, dOutput2PositionX,
                    dOutput2PositionY);

                return optimalPositions;
            }
            else
            {
                throw new InvalidOperationException("Tried to schedule a merge for a non-merge command!");
            }
        }
    }
}
