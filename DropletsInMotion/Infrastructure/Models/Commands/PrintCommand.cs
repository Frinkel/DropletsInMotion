using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

public class PrintCommand : ICommand
{
    public List<object> Arguments { get; }
    public int Line { get; set; }
    public int Column { get; set; }

    public PrintCommand(List<object> arguments)
    {
        Arguments = arguments;
    }

    public void Evaluate(Dictionary<string, double> variableValues)
    {
        var output = new System.Text.StringBuilder();

        for (int i = 0; i < Arguments.Count; i++)
        {
            var arg = Arguments[i];

            if (arg is ArithmeticExpression arithmeticExpression)
            {
                double result = arithmeticExpression.Evaluate(variableValues);
                output.Append(result);
            }
            else if (arg is BooleanExpression booleanExpression)
            {
                bool result = booleanExpression.Evaluate(variableValues);
                output.Append(result);
            }
            else
            {
                output.Append(arg.ToString());
            }

            if (i < Arguments.Count - 1)
            {
                output.Append(" ");
            }
        }

        Console.WriteLine(output.ToString());
    }

    public List<string> GetInputVariables()
    {
        var arithmeticVars = Arguments.OfType<ArithmeticExpression>().SelectMany(expr => expr.GetVariables());
        var booleanVars = Arguments.OfType<BooleanExpression>().SelectMany(expr => expr.GetVariables());

        return arithmeticVars.Concat(booleanVars).ToList();
    }

    public List<string> GetOutputVariables()
    {
        return new List<string>();
    }

    public override string ToString()
    {
        return $"Print({string.Join(", ", Arguments)})";
    }
}