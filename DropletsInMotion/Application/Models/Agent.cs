using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Platform;

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

        public bool IsMoveApplicable(Types.RouteAction action, State state)
        {
            var contamination = state.ContaminationMap;
            var agents = state.Agents;
            var deltaX = PositionX + action.DropletXDelta;
            var deltaY = PositionY + action.DropletYDelta;

            //Check out of bounds
            if (deltaX < 0 || deltaX >= contamination.GetLength(0) || deltaY < 0 || deltaY >= contamination.GetLength(1))
            {
                return false;
            }

            // check for contaminations
            if (contamination[deltaX, deltaY] != 0 && contamination[deltaX, deltaY] != SubstanceId)
            {
                return false;
            }

            if (state.Parent != null &&
                action.Type != Types.ActionType.NoOp &&
                deltaX == state.Parent.Agents[DropletName].PositionX &&
                deltaY == state.Parent.Agents[DropletName].PositionY)
            {
                return false;
            }


            //Check for going near other agents of the same substance
            foreach (var otherAgentKvp in agents)
            {
                var otherAgent = otherAgentKvp.Value;
                if (otherAgent.SubstanceId != SubstanceId || otherAgent.DropletName == DropletName) continue;
                if (Math.Abs(otherAgent.PositionX - deltaX) <= 1 && Math.Abs(otherAgent.PositionY - deltaY) <= 1)
                {
                    return false;
                }
            }

            return true;
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
