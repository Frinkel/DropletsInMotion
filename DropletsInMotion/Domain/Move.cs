﻿using System.Xml.Linq;

namespace DropletsInMotion.Domain
{
    public class Move : ICommand
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
