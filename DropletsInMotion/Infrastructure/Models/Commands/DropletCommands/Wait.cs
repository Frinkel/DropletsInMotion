using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Wait : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public double Time { get; set; }
        public ArithmeticExpression TimeExpression { get; }

        public Wait(double time)
        {
            Time = time;
        }
        public Wait(ArithmeticExpression timeExpression)
        {
            TimeExpression = timeExpression;
        }

        public override string ToString()
        {
            return $"Wait(Time: {TimeExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            Time = TimeExpression.Evaluate(variableValues);
        }
        public List<string> GetOutputVariables()
        {
            return new List<string> { };
        }

        public List<string> GetInputVariables()
        {
            return TimeExpression.GetVariables();
        }
    }
}
