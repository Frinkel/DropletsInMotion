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
        public int NewPositionX { get; }
        public int NewPositionY { get; }
        public double Time { get; }

        public Store(string dropletName, int newPositionX, int newPositionY, double time)
        {
            DropletName = dropletName;
            NewPositionX = newPositionX;
            NewPositionY = newPositionY;
            Time = time;
        }

        public override string ToString()
        {
            return $"Store(DropletName: {DropletName}, NewPositionX: {NewPositionX}, NewPositionY: {NewPositionY}, Time: {Time})";
        }
    }
}
