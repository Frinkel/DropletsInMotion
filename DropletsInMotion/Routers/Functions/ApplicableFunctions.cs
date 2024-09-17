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

                case Types.ActionType.Split:
                    break;

                case Types.ActionType.Merge:
                    break;

                default:
                    throw new InvalidOperationException("An illegal action was tried!");
            }

            return false;
        }
        private static bool IsMoveApplicable(Types.RouteAction action, Agent agent, Dictionary<string, Agent> agents, byte[,] contamination)
        {
            var deltaX = agent.PositionX + action.Droplet1XDelta;
            var deltaY = agent.PositionY + action.Droplet1YDelta;
        }
    }
}
