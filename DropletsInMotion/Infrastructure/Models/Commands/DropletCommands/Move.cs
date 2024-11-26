using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Move : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
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
            return $"Move(DropletName: {DropletName}, PositionX: {PositionX}, PositionY: {PositionY})";
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


        public List<string> GetOutputVariables()
        {
            return GetOutputDroplets();
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
