using DropletsInMotion.Application.Models;

namespace DropletsInMotion.Application.Services.Routers.Models
{
    public class RoutableAgent : Agent
    {
        public RoutableAgent(string dropletName, int positionX, int positionY, double volume) : base(dropletName, positionX, positionY, volume)
        {
        }

        internal void Execute(Types.RouteAction action)
        {
            switch (action.Type)
            {
                case Types.ActionType.NoOp:
                    break;
                case Types.ActionType.Move:
                    PositionX += action.DropletXDelta;
                    PositionY += action.DropletYDelta;
                    //Move(action.DropletXDelta, action.DropletYDelta);
                    break;
                default:
                    throw new InvalidOperationException("Invalid action tried to be executed! (Agent.cs)");
            }
        }
    }
}
