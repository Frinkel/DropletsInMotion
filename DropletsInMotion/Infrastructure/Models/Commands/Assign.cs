using System;
using System.Collections.Generic;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class Assign : ICommand
    {
        public string VariableName { get; }
        public ArithmeticExpression ValueExpression { get; }

        public Assign(string variableName, ArithmeticExpression valueExpression)
        {
            VariableName = variableName;
            ValueExpression = valueExpression;
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            double value = ValueExpression.Evaluate(variableValues);
            variableValues[VariableName] = value;
            
        }

        public override string ToString()
        {
            return $"{VariableName} = {ValueExpression}";
        }

        public List<string> GetVariables()
        {
            var res = ValueExpression.GetVariables();
            res.Add(VariableName);
            return res;
        }
    }
}