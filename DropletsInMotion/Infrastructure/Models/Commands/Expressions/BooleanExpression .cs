namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public abstract class BooleanExpression
    {
        public abstract bool Evaluate(Dictionary<string, double> variableValues);
        public abstract List<string> GetVariables();

    }
}
