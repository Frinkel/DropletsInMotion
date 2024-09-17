using DropletsInMotion.Domain;

namespace DropletsInMotion.Routers.Models
{
    public class Agent : Droplet
    {
        private static byte _nextSubstanceId = 1;
        public byte SubstanceId;
        public ICommand Command;

        public Agent(string dropletName, int positionX, int positionY, double volume) : base(dropletName, positionX, positionY, volume)
        {
            SubstanceId = GetNextSubstanceId();
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
    }
}
