using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Waste : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }

        public Waste(string dropletName, int newPositionX, int newPositionY)
        {
            DropletName = dropletName;
            PositionX = newPositionX;
            PositionY = newPositionY;
        }
        public Waste(string dropletName, ArithmeticExpression newPositionXExpression, ArithmeticExpression newPositionYExpression)
        {
            DropletName = dropletName;
            PositionXExpression = newPositionXExpression;
            PositionYExpression = newPositionYExpression;
        }

        public override string ToString()
        {
            return $"Waste(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { };
        }
        public void Evaluate(Dictionary<string, double> variableValues)
        {
            PositionX = (int)PositionXExpression.Evaluate(variableValues);
            PositionY = (int)PositionYExpression.Evaluate(variableValues);
        }

        public List<string> GetOutputVariables()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetInputVariables()
        {
            var res = PositionXExpression.GetVariables();
            res.AddRange(PositionYExpression.GetVariables());
            res.AddRange(GetInputDroplets());
            return res;
        }
    }
}
