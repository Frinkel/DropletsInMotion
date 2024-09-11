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
        public string Name { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public int DistanceX { get; }
        public int DistanceY { get; }
        public int RepeatTimes { get; }

        public Mix(string name, int positionX, int positionY, int distanceX, int distanceY, int repeatTimes)
        {
            Name = name;
            PositionX = positionX;
            PositionY = positionY;
            DistanceX = distanceX;
            DistanceY = distanceY;
            RepeatTimes = repeatTimes;
        }
        public override string ToString()
        {
            return $"Mix(Name: {Name}, PositionX: {PositionX}, PositionY: {PositionY}), DistanceX: {DistanceX}, DistanceY: {DistanceY}), RepeatTimes: {RepeatTimes}";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { Name };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { Name };
        }

    }
}
