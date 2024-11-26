using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class SplitByRatio : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string InputName { get; }
        public string OutputName1 { get; }
        public string OutputName2 { get; }
        public int PositionX1 { get; set; }
        public int PositionY1 { get; set; }
        public int PositionX2 { get; set; }
        public int PositionY2 { get; set; }
        public double Ratio { get; set; }
        public ArithmeticExpression PositionX1Expression { get; }
        public ArithmeticExpression PositionY1Expression { get; }
        public ArithmeticExpression PositionX2Expression { get; }
        public ArithmeticExpression PositionY2Expression { get; }
        public ArithmeticExpression RatioExpression { get; }


        public SplitByRatio(string inputName, string outputName1, string outputName2, int positionX1, int positionY1, int positionX2, int positionY2, double ratio)
        {
            InputName = inputName;
            OutputName1 = outputName1;
            OutputName2 = outputName2;
            PositionX1 = positionX1;
            PositionY1 = positionY1;
            PositionX2 = positionX2;
            PositionY2 = positionY2;
            Ratio = ratio;
        }

        public SplitByRatio(string inputName, string outputName1, string outputName2, ArithmeticExpression positionX1Expression, ArithmeticExpression positionY1Expression, ArithmeticExpression positionX2Expression, ArithmeticExpression positionY2Expression, ArithmeticExpression ratioExpression)
        {
            InputName = inputName;
            OutputName1 = outputName1;
            OutputName2 = outputName2;
            PositionX1Expression = positionX1Expression;
            PositionY1Expression = positionY1Expression;
            PositionX2Expression = positionX2Expression;
            PositionY2Expression = positionY2Expression;
            RatioExpression = ratioExpression;
        }

        public override string ToString()
        {
            return $"SplitByRatio(InputName: {InputName}, OutputName1: {OutputName1}, OutputName2: {OutputName2}, PositionX1: {PositionX1Expression}, PositionY1: {PositionY1Expression}, PositionX2: {PositionX2Expression}, PositionY2: {PositionY2Expression}, Ratio: {Ratio})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { InputName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { OutputName1, OutputName2 };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            PositionX1 = (int)PositionX1Expression.Evaluate(variableValues);
            PositionY1 = (int)PositionY1Expression.Evaluate(variableValues);
            PositionX2 = (int)PositionX2Expression.Evaluate(variableValues);
            PositionY2 = (int)PositionY2Expression.Evaluate(variableValues);
            Ratio = RatioExpression.Evaluate(variableValues);
        }

        public List<string> GetInputVariables()
        {
            var x = PositionX1Expression.GetVariables();
            x.AddRange(PositionY1Expression.GetVariables());
            x.AddRange(PositionX2Expression.GetVariables());
            x.AddRange(PositionY2Expression.GetVariables());
            x.AddRange(RatioExpression.GetVariables());
            x.AddRange(GetInputDroplets());
            return x;
        }

        public List<string> GetOutputVariables() {
            return GetOutputDroplets();
        }
    }
}
