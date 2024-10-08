using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Store : IDropletCommand
    {
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Time { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }
        public ArithmeticExpression TimeExpression { get; }

        public Store(string dropletName, int newPositionX, int newPositionY, double time)
        {
            DropletName = dropletName;
            PositionX = newPositionX;
            PositionY = newPositionY;
            Time = time;
        }
        public Store(string dropletName, ArithmeticExpression newPositionXExpression, ArithmeticExpression newPositionYExpression, ArithmeticExpression timeExpression)
        {
            DropletName = dropletName;
            PositionXExpression = newPositionXExpression;
            PositionYExpression = newPositionYExpression;
            TimeExpression = timeExpression;
        }

        public override string ToString()
        {
            return $"Store(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression}, Time: {TimeExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }
        public void Evaluate(Dictionary<string, double> variableValues)
        {
            PositionX = (int)PositionXExpression.Evaluate(variableValues);
            PositionY = (int)PositionYExpression.Evaluate(variableValues);
            Time = TimeExpression.Evaluate(variableValues);
        }

        public List<string> GetOutputVariables()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetInputVariables()
        {
            var res = PositionXExpression.GetVariables();
            res.AddRange(PositionYExpression.GetVariables());
            res.AddRange(TimeExpression.GetVariables());
            res.AddRange(GetInputDroplets());
            return res;
        }
    }
}
