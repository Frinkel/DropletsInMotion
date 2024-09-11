using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class SplitByRatio : ICommand
    {
        public string Name { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Volume { get; set; }

        public SplitByRatio(string name, int positionX, int positionY, double volume)
        {
            Name = name;
            PositionX = positionX;
            PositionY = positionY;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"Droplet(Name: {Name}, PositionX: {PositionX}, PositionY: {PositionY}, Volume: {Volume})";
        }
    }
}
