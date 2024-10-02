using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Merge : IDropletCommand
    {
        public string InputName1 { get; }
        public string InputName2 { get; }
        public string OutputName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }

        public Merge(string inputName1, string inputName2, string outputName, int positionX, int positionY)
        {
            InputName1 = inputName1;
            InputName2 = inputName2;
            OutputName = outputName;
            PositionX = positionX;
            PositionY = positionY;
        }
        public Merge(string inputName1, string inputName2, string outputName,
            ArithmeticExpression positionXExpression, ArithmeticExpression positionYExpression)
        {
            InputName1 = inputName1;
            InputName2 = inputName2;
            OutputName = outputName;
            PositionXExpression = positionXExpression;
            PositionYExpression = positionYExpression;
        }

        public override string ToString()
        {
            return $"Merge(InputName1: {InputName1}, InputName2: {InputName2}, OutputName: {OutputName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { InputName1, InputName2 };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { OutputName };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            PositionX = (int)PositionXExpression.Evaluate(variableValues);
            PositionY = (int)PositionYExpression.Evaluate(variableValues);
        }
    }
}
