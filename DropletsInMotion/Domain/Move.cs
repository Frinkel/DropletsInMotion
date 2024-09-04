using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class Move
    {
        public string DropletName { get; }
        public int NewPositionX { get; }
        public int NewPositionY { get; }

        public Move(string dropletName, int newPositionX, int newPositionY)
        {
            DropletName = dropletName;
            NewPositionX = newPositionX;
            NewPositionY = newPositionY;
        }

        public override string ToString()
        {
            return $"Move(DropletName: {DropletName}, NewPositionX: {NewPositionX}, NewPositionY: {NewPositionY})";
        }
    }
}
