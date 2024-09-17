using DropletsInMotion.Routers.Models;

namespace DropletsInMotion.Routers.Functions
{
    public static class ApplicableFunctions
    {
        public static bool IsApplicable(Tuple<Types.RouteAction, List<string>> action, Dictionary<string, Agent> agents, byte[,] contamination)
        {
            switch (action.Item1.Type)
            {
                case Types.ActionType.NoOp:
                    return true;

                case Types.ActionType.Move:
                    Agent agent = agents[action.Item2.First()];
                    return IsMoveApplicable(action.Item1, agent, agents, contamination);
                    break;


                default:
                    throw new InvalidOperationException("An illegal action was tried!");
            }

            return false;
        }
        public static bool IsMoveApplicable(Types.RouteAction action, Agent agent, Dictionary<string, Agent> agents, byte[,] contamination)
        {
            var deltaX = agent.PositionX + action.DropletXDelta;
            var deltaY = agent.PositionY + action.DropletYDelta;
            //Check out of bounds
            if (deltaX < 0 || deltaX >= contamination.GetLength(0) || deltaY < 0 || deltaY >= contamination.GetLength(1))
            {
                return false;
            }
            // check for contaminations
            if (contamination[deltaX, deltaY] != 0 && contamination[deltaX, deltaY] != agent.SubstanceId)
            {
                return false;
            }
            //Check for going near other agents of the same substance
            foreach (var otherAgentKvp in agents )
            {
                var otherAgent = otherAgentKvp.Value;
                if (otherAgent.SubstanceId != agent.SubstanceId) continue;
                if (Math.Abs(otherAgent.PositionX - deltaX) <= 1 || Math.Abs(otherAgent.PositionY - deltaY) <= 1)
                {
                    return false;
                }
            }
            //returns true
            return true;
        }

        public static byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            void ApplyIfInBounds(int xPos, int yPos)
            {
                if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
                {
                    contaminationMap[xPos, yPos] = (byte)(contaminationMap[xPos, yPos] == 0 ? agent.SubstanceId : 255);
                }
            }

            ApplyIfInBounds(x, y);
            ApplyIfInBounds(x + 1, y);
            ApplyIfInBounds(x - 1, y);
            ApplyIfInBounds(x, y + 1);
            ApplyIfInBounds(x, y - 1);

            ApplyIfInBounds(x + 1, y + 1);
            ApplyIfInBounds(x + 1, y - 1);
            ApplyIfInBounds(x - 1, y + 1);
            ApplyIfInBounds(x - 1, y - 1);

            return contaminationMap;
        }
    }
}
