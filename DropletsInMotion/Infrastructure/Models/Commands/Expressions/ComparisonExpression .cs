using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            double leftValue = Left.Evaluate(variableValues);
            double rightValue = Right.Evaluate(variableValues);

            //TODO consider adding a tolerance for floating point comparison
            return Operator switch
            {
                ">" => leftValue > rightValue,
                "<" => leftValue < rightValue,
                "==" => leftValue == rightValue,
                "!=" => leftValue != rightValue,
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
