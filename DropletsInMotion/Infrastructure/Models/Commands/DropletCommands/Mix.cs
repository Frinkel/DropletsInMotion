using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Mix : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int RepeatTimes { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }
        public ArithmeticExpression WidthExpression { get; }
        public ArithmeticExpression HeightExpression { get; }
        public ArithmeticExpression RepeatTimesExpression { get; }

        public Mix(string dropletName, int positionX, int positionY, int width, int height, int repeatTimes)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
            Width = width;
            Height = height;
            RepeatTimes = repeatTimes;
        }

        public Mix(string dropletName, ArithmeticExpression positionXExpression, ArithmeticExpression positionYExpression, ArithmeticExpression widthExpression, ArithmeticExpression heightExpression, ArithmeticExpression repeatTimesExpression)
        {
            DropletName = dropletName;
            PositionXExpression = positionXExpression;
            PositionYExpression = positionYExpression;
            WidthExpression = widthExpression;
            HeightExpression = heightExpression;
            RepeatTimesExpression = repeatTimesExpression;
        }
        public override string ToString()
        {
            return $"Mix(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression}), DistanceX: {WidthExpression}, DistanceY: {HeightExpression}), RepeatTimes: {RepeatTimesExpression}";
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
            Width = (int)WidthExpression.Evaluate(variableValues);
            Height = (int)HeightExpression.Evaluate(variableValues);
            RepeatTimes = (int)RepeatTimesExpression.Evaluate(variableValues);
        }

        public List<string> GetOutputVariables()
        {
            return GetOutputDroplets();
        }

        public List<string> GetInputVariables()
        {
            var res = PositionXExpression.GetVariables();
            res.AddRange(PositionYExpression.GetVariables());
            res.AddRange(WidthExpression.GetVariables());
            res.AddRange(HeightExpression.GetVariables());
            res.AddRange(RepeatTimesExpression.GetVariables());
            res.AddRange(GetInputDroplets());
            return res;
        }

    }
}
