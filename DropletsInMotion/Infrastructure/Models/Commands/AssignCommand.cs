using System;
using System.Collections.Generic;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class AssignCommand : ICommand
    {
        public string VariableName { get; }
        public ArithmeticExpression ValueExpression { get; }

        public AssignCommand(string variableName, ArithmeticExpression valueExpression)
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

        public List<string> GetInputVariables()
        {
            return ValueExpression.GetVariables();
        }

        public List<string> GetOutputVariables()
        {
            return new List<string>() { VariableName };
        }
    }
}