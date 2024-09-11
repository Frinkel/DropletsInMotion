using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class Store : ICommand
    {
        public string DropletName { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public double Time { get; }

        public Store(string dropletName, int newPositionX, int newPositionY, double time)
        {
            DropletName = dropletName;
            PositionX = newPositionX;
            PositionY = newPositionY;
            Time = time;
        }

        public override string ToString()
        {
            return $"Store(DropletName: {DropletName}, PositionX: {PositionX}, PositionY: {PositionY}, Time: {Time})";
        }
    }
}
