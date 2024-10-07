using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Application.Models
{
    public class Agent : Droplet
    {
        private static byte _nextSubstanceId = 1;
        public byte SubstanceId;

        public Agent(string dropletName, int positionX, int positionY, double volume) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = GetNextSubstanceId();
        }

        public Agent(string dropletName, int positionX, int positionY, double volume, byte substanceId) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = substanceId;
        }

        private static byte GetNextSubstanceId()
        {
            if (_nextSubstanceId == 254)
            {
                throw new InvalidOperationException("Maximum number of unique agents reached.");
            }

            return _nextSubstanceId++;
        }

        public override string ToString()
        {
            return $"Agent({base.ToString()}, SubstanceId: {SubstanceId})";
        }

        public object Clone()
        {
            return new Agent(DropletName, PositionX, PositionY, Volume, SubstanceId);
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

        public static void ResetSubstanceId()
        {
            _nextSubstanceId = 1;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionX, PositionY);
        }
    }
}
