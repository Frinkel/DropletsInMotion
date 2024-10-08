using System;
using System.Collections.Generic;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class IfCommand : ICommand
    {
        public BooleanExpression Condition { get; }
        public List<ICommand> IfBlockCommands { get; }
        public List<ICommand> ElseBlockCommands { get; }
        public bool Evaluation { get; set; }
        public bool HasBeenEvaluated { get; set; }

        public IfCommand(BooleanExpression condition, List<ICommand> ifBlockCommands, List<ICommand> elseBlockCommands = null)
        {
            Condition = condition;
            IfBlockCommands = ifBlockCommands;
            ElseBlockCommands = elseBlockCommands ?? new List<ICommand>();
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            if (HasBeenEvaluated) return;
            HasBeenEvaluated = true;
            Evaluation = Condition.Evaluate(variableValues);
        }

        public override string ToString()
        {
            return $"If({Condition})";
        }

        public List<string> GetInputVariables()
        {
            var variables = new List<string>();
            variables.AddRange(Condition.GetVariables());
            foreach (var command in IfBlockCommands)
            {
                variables.AddRange(command.GetInputVariables());
            }
            foreach (var command in ElseBlockCommands)
            {
                variables.AddRange(command.GetInputVariables());
            }
            return variables;
        }

        public List<string> GetOutputVariables()
        {
            var variables = new List<string>();
            foreach (var command in IfBlockCommands)
            {
                variables.AddRange(command.GetOutputVariables());
            }
            foreach (var command in ElseBlockCommands)
            {
                variables.AddRange(command.GetOutputVariables());
            }
            return variables;
        }
    }
}
