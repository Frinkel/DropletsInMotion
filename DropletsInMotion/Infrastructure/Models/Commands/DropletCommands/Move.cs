using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Move : IDropletCommand
    {
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }

        public Move(string dropletName, int positionX, int positionY)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
        }

        public Move(string dropletName, ArithmeticExpression positionXExpression, ArithmeticExpression positionYExpression)
        {
            DropletName = dropletName;
            PositionXExpression = positionXExpression;
            PositionYExpression = positionYExpression;
        }

        public override string ToString()
        {
            return $"Move(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression})";
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
        }

        public List<string> GetVariables()
        {
            var x = PositionXExpression.GetVariables();
            x.AddRange(PositionYExpression.GetVariables());
            return x;
        }
    }
}
