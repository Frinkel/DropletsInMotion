using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DropletsInMotion.Domain
{
    public class Mix : ICommand
    {
        //Mix(name, posX, posY, distanceX, distanceY, repeatTimes)
        public string DropletName { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public int Width { get; }
        public int Height { get; }
        public int RepeatTimes { get; }

        public Mix(string dropletName, int positionX, int positionY, int width, int height, int repeatTimes)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
            Width = width;
            Height = height;
            RepeatTimes = repeatTimes;
        }
        public override string ToString()
        {
            return $"Mix(DropletName: {DropletName}, PositionX: {PositionX}, PositionY: {PositionY}), DistanceX: {Width}, DistanceY: {Height}), RepeatTimes: {RepeatTimes}";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }

    }
}
