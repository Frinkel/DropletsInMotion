namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public abstract class ArithmeticExpression
    {
        public abstract double Evaluate(Dictionary<string, double> variableValues);
        public abstract List<string> GetVariables();
    }
}
