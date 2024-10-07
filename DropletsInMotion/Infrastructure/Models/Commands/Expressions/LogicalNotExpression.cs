namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class LogicalNotExpression : BooleanExpression
    {
        private BooleanExpression Operand { get; }

        public LogicalNotExpression(BooleanExpression operand)
        {
            Operand = operand;
        }

        public override bool Evaluate(Dictionary<string, double> variableValues)
        {
            return !Operand.Evaluate(variableValues);
        }

        public override string ToString()
        {
            return $"!({Operand})";
        }
    }
}

