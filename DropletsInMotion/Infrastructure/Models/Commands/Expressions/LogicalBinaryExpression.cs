namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class LogicalBinaryExpression : BooleanExpression
    {
        private BooleanExpression Left { get; }
        private BooleanExpression Right { get; }
        private string Operator { get; }

        public LogicalBinaryExpression(BooleanExpression left, string op, BooleanExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override bool Evaluate(Dictionary<string, double> variableValues)
        {
            bool leftValue = Left.Evaluate(variableValues);
            bool rightValue = Right.Evaluate(variableValues);

            return Operator switch
            {
                "&&" => leftValue && rightValue,
                "||" => leftValue || rightValue,
                _ => throw new InvalidOperationException($"Unknown logical operator: {Operator}")
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

