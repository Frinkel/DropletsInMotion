using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Application.Services;

namespace DropletsInMotion.Application.Models
{
    public class Agent : Droplet
    {
        private IContaminationService _contaminationService;
        private static int _nextSubstanceId = 1;
        public int SubstanceId;
        public LinkedList<(int x, int y)> SnakeBody = new LinkedList<(int x, int y)>();
        private static double _minimumMovementVolume = 0;
        private static double _minSize1x1 = 0;
        private static double _minSize2x2 = 0;
        private static double _minSize3x3 = 0;

        public Agent(string dropletName, int positionX, int positionY, double volume, IContaminationService contaminationService) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = GetNextSubstanceId();
            SnakeBody.AddFirst((positionX, positionY));
            _contaminationService = contaminationService;
        }

        public Agent(string dropletName, int positionX, int positionY, double volume, int substanceId, IContaminationService contaminationService) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = substanceId;
            SnakeBody.AddFirst((positionX, positionY));
            _contaminationService = contaminationService;
        }

        public Agent(string dropletName, int positionX, int positionY, double volume, int substanceId, LinkedList<(int x, int y)> snakeBody, IContaminationService contaminationService) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = substanceId;
            SnakeBody = snakeBody;
            _contaminationService = contaminationService;
        }

        private static int GetNextSubstanceId()
        {
            if (_nextSubstanceId == 254)
            {
                throw new RuntimeException("Maximum number of unique agents reached.");
            }

            return _nextSubstanceId++;
        }

        public override string ToString()
        {
            return $"Agent({base.ToString()}, SubstanceId: {SubstanceId})";
        }

        public object Clone()
        {
            LinkedList<(int x, int y)> clonedSnakeBody = new LinkedList<(int x, int y)>();

            foreach (var bodyPart in SnakeBody)
            {
                clonedSnakeBody.AddLast(bodyPart);
            }

            return new Agent(DropletName, PositionX, PositionY, Volume, SubstanceId, clonedSnakeBody, _contaminationService);
        }

        internal void Execute(Types.RouteAction action)
        {
            switch (action.Type)
            {
                case Types.ActionType.NoOp:
                    break;
                case Types.ActionType.Move:
                    AddToSnake(PositionX + action.DropletXDelta,
                        PositionY + action.DropletYDelta);
                    break;
                default:
                    throw new RuntimeException("Invalid action tried to be executed! (Agent.cs)");
            }
        }

        public bool IsMoveApplicable(Types.RouteAction action, Dictionary<string, Agent> otherAgents, List<int>[,] otherContaminationMap, State state)
        {
            if (action.Type == Types.ActionType.NoOp) return false; // TODO: We do not consider states where we NOOP for now, so we do not consider the action!


            var contamination = state.ContaminationMap;
            var deltaX = PositionX + action.DropletXDelta;
            var deltaY = PositionY + action.DropletYDelta;

            // Check out of bounds
            if (deltaX < 0 || deltaX >= contamination.GetLength(0) || deltaY < 0 || deltaY >= contamination.GetLength(1))
            {
                return false;
            }

            // Check for contaminations
            if (_contaminationService.IsConflicting(state.GetContamination(deltaX, deltaY), SubstanceId))
            {
                return false;
            }

            if (_contaminationService.IsConflicting(otherContaminationMap, deltaX, deltaY, SubstanceId))
            {
                return false;
            }


            // Check for going near other agents of the same substance
            foreach (var otherAgentKvp in otherAgents)
            {
                var otherAgent = otherAgentKvp.Value;
                
                if (otherAgent.DropletName == DropletName) continue;

                var otherAgentSnake = otherAgent.SnakeBody;


                foreach (var (x, y) in otherAgentSnake)
                {
                    if (Math.Abs(x - deltaX) <= 1 && Math.Abs(y - deltaY) <= 1)
                    {
                        return false;
                    }
                }

                foreach (var (x, y) in SnakeBody)
                {

                    if (Math.Abs(x - otherAgent.PositionX) <= 1 && Math.Abs(y - otherAgent.PositionY) <= 1)
                    {
                        return false;
                    }
                }

                if (otherAgent.GetMaximumSnakeLength() > otherAgentSnake.Count)
                {
                    var allOtherAgentPosition = otherAgent.GetAllAgentPositions();
                    foreach (var (x, y) in allOtherAgentPosition)
                    {
                        if (Math.Abs(x - deltaX) <= 1 && Math.Abs(y - deltaY) <= 1)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void AddToSnake(int x, int y)
        {
            PositionX = x;
            PositionY = y;
            SnakeBody.AddFirst((x, y));
            int maximumSnakeLength = GetMaximumSnakeLength();
            if (SnakeBody.Count > maximumSnakeLength + 1)
            {
                SnakeBody.RemoveLast();
            }
        }

        public void RemoveFromSnake()
        {
            if (SnakeBody.Count > 1)
            {
                SnakeBody.RemoveLast();
            }
        }

        public static void ResetSubstanceId()
        {
            _nextSubstanceId = 1;
        }

        public static void SetMinimumMovementVolume(double minimumMovementVolume)
        {
            _minimumMovementVolume = minimumMovementVolume;
        }

        public static void SetMinSize1x1(double minSize1x1)
        {
            _minSize1x1 = minSize1x1;
        }

        public static void SetMinSize2x2(double minSize2x2)
        {
            _minSize2x2 = minSize2x2;
        }

        public static void SetMinSize3x3(double minSize3x3)
        {
            _minSize3x3 = minSize3x3;
        }

        public List<(int x, int y)> GetAllAgentPositions()
        {
            int size = GetAgentSize();
            var positions = new List<(int x, int y)>();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    positions.Add((PositionX + i,PositionY + j));
                }
            }


            return positions;
        }

        public int GetAgentSize()
        {
            int size = 1;
            if (Volume >= _minSize2x2)
            {
                size = 2;
            }
            if (Volume >= _minSize3x3)
            {
                size = 3;
            }

            return size;
        }

        public int GetMaximumSnakeLength()
        {
            int maximumSnakeLength = (int)(Volume / _minimumMovementVolume);
            return maximumSnakeLength;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionX, PositionY);
        }
    }
}
