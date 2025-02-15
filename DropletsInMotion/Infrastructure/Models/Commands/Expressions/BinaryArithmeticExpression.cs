﻿using DropletsInMotion.Infrastructure.Exceptions;

namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public class BinaryArithmeticExpression : ArithmeticExpression
    {
        private ArithmeticExpression Left { get; }
        private ArithmeticExpression Right { get; }
        private string Operator { get; }

        public BinaryArithmeticExpression(ArithmeticExpression left, string op, ArithmeticExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        // Method to evaluate the arithmetic expression
        public override double Evaluate(Dictionary<string, double> variableValues)
        {
            double leftValue = Left.Evaluate(variableValues);
            double rightValue = Right.Evaluate(variableValues);

            return Operator switch
            {
                "+" => leftValue + rightValue,
                "-" => leftValue - rightValue,
                "*" => leftValue * rightValue,
                "/" => rightValue != 0 ? leftValue / rightValue : throw new DivideByZeroException(),
                _ => throw new RuntimeException($"Unknown operator: {Operator}")
            };
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override List<string> GetVariables()
        {
            var res = Left.GetVariables();
            res.AddRange(Right.GetVariables());
            return res;
        }
    }
}
