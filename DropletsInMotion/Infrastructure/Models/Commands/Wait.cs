namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class Wait : ICommand
    {
        public double Time { get; }

        public Wait(double time)
        {
            Time = time;
        }

        public override string ToString()
        {
            return $"Wait(Time: {Time})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { };
        }
    }
}
