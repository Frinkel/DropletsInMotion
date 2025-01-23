namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class ComparisonExpression : BooleanExpression
    {
        private ArithmeticExpression Left { get; }
        private ArithmeticExpression Right { get; }
        private string Operator { get; }

        public ComparisonExpression(ArithmeticExpression left, string op, ArithmeticExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override bool Evaluate(Dictionary<string, double> variableValues)
        {
            const double epsilon = 1e-10;

            double leftValue = Left.Evaluate(variableValues);
            double rightValue = Right.Evaluate(variableValues);

            return Operator switch
            {
                ">" => leftValue > rightValue + epsilon,
                "<" => leftValue < rightValue - epsilon,
                "==" => Math.Abs(leftValue - rightValue) < epsilon,
                "!=" => Math.Abs(leftValue - rightValue) >= epsilon,
                _ => throw new InvalidOperationException($"Unknown comparison operator: {Operator}")
            };
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override List<string> GetVariables()
        {
            List<string> variables = Left.GetVariables();
            variables.AddRange(Right.GetVariables());
            return variables.Distinct().ToList();
        }
    }

}
