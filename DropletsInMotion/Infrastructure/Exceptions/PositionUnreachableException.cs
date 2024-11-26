using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class PositionUnreachableException : Exception
    {

        public List<Agent> Agents { get; }

        //public List<(int x, int y)> CurrentPositions { get; }
        public List<(int x, int y)> TargetPositions { get;  }

        public PositionUnreachableException(string message, List<Agent> agents, List<(int x, int y)> targetPositions) : base(message)
        {
            Agents = agents;
            TargetPositions = targetPositions;
        }

        public PositionUnreachableException(string message, List<Agent> agents, List<(int x, int y)> targetPositions, Exception innerException) : base(message, innerException)
        {
            Agents = agents;
            TargetPositions = targetPositions;
        }
    }
}
