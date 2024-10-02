namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class UnaryNegationExpression : ArithmeticExpression
    {
        private ArithmeticExpression Operand { get; }

        public UnaryNegationExpression(ArithmeticExpression operand)
        {
            Operand = operand;
        }

        public override double Evaluate(Dictionary<string, double> variableValues)
        {
            return -Operand.Evaluate(variableValues);
        }

        public override string ToString()
        {
            return $"-({Operand})";
        }
    }
}