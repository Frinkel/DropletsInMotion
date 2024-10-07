namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class LiteralExpression : ArithmeticExpression
    {
        private double Value { get; }

        public LiteralExpression(double value)
        {
            Value = value;
        }

        public override double Evaluate(Dictionary<string, double> variableValues)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override List<string> GetVariables()
        {
            return new List<string>();
        }
    }
}