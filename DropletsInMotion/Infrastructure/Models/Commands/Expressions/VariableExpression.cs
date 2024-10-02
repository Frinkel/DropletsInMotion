namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class VariableExpression : ArithmeticExpression
    {
        private string VariableName { get; }

        public VariableExpression(string variableName)
        {
            VariableName = variableName;
        }

        public override double Evaluate(Dictionary<string, double> variableValues)
        {
            if (variableValues.ContainsKey(VariableName))
            {
                return variableValues[VariableName];
            }
            else
            {
                throw new InvalidOperationException($"Variable '{VariableName}' not found.");
            }
        }

        public override string ToString()
        {
            return VariableName;
        }
    }
}