using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Routers.Models;

public class Types
{
    // Define the ActionType enum
    public enum ActionType
    {
        NoOp,
        Move,
        Split,
        Merge,
    }

    // Define the Action class, equivalent to the enum with associated data
    public class RouteAction
    {
        // Properties for each field in the Java version
        public string Name { get; }
        public ActionType Type { get; }
        public int Droplet1XDelta { get; }
        public int Droplet1YDelta { get; }
        public int Droplet2XDelta { get; }
        public int Droplet2YDelta { get; }

        // Constructor to initialize an action
        public RouteAction(string name, ActionType type, int droplet1XDelta, int droplet1YDelta, int droplet2XDelta = 0, int droplet2YDelta = 0)
        {
            Name = name;
            Type = type;
            Droplet1XDelta = droplet1XDelta;
            Droplet1YDelta = droplet1YDelta;
            Droplet2XDelta = droplet2XDelta;
            Droplet2YDelta = droplet2YDelta;
        }

        // Define all possible actions as static readonly fields
        public static readonly RouteAction NoOp = new RouteAction("NoOp", ActionType.NoOp, 0, 0);

        public static readonly RouteAction MoveUp = new RouteAction("moveUp", ActionType.Move, 0, -1);
        public static readonly RouteAction MoveDown = new RouteAction("moveDown", ActionType.Move, 0, 1);
        public static readonly RouteAction MoveRight = new RouteAction("moveRight", ActionType.Move, 1, 0);
        public static readonly RouteAction MoveLeft = new RouteAction("moveLeft", ActionType.Move, -1, 0);


    }
}

