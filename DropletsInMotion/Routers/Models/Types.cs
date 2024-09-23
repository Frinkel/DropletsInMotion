using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Routers.Models;

public class Types
{
    public enum ActionType
    {
        NoOp,
        Move,
    }

    public class RouteAction
    {
        public string Name { get; }
        public ActionType Type { get; }
        public int DropletXDelta { get; }
        public int DropletYDelta { get; }

        public RouteAction(string name, ActionType type, int dropletXDelta, int dropletYDelta)
        {
            Name = name;
            Type = type;
            DropletXDelta = dropletXDelta;
            DropletYDelta = dropletYDelta;
        }

        public static readonly RouteAction NoOp = new RouteAction("NoOp", ActionType.NoOp, 0, 0);

        public static readonly RouteAction MoveUp = new RouteAction("moveUp", ActionType.Move, 0, -1);
        public static readonly RouteAction MoveDown = new RouteAction("moveDown", ActionType.Move, 0, 1);
        public static readonly RouteAction MoveRight = new RouteAction("moveRight", ActionType.Move, 1, 0);
        public static readonly RouteAction MoveLeft = new RouteAction("moveLeft", ActionType.Move, -1, 0);

        public static readonly List<RouteAction> PossiblActions = new List<RouteAction>() {NoOp, MoveUp, MoveDown, MoveRight, MoveLeft};
    }
}

