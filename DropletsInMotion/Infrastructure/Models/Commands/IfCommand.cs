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

        public IfCommand(BooleanExpression condition, List<ICommand> ifBlockCommands, List<ICommand> elseBlockCommands = null)
        {
            Condition = condition;
            IfBlockCommands = ifBlockCommands;
            ElseBlockCommands = elseBlockCommands ?? new List<ICommand>();
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            bool conditionResult = Condition.Evaluate(variableValues);

            // Evaluate either if-block or else-block depending on the condition result
            var commandsToExecute = conditionResult ? IfBlockCommands : ElseBlockCommands;
            foreach (var command in commandsToExecute)
            {
                command.Evaluate(variableValues);
            }
        }

        public override string ToString()
        {
            return $"If({Condition})";
        }

        public List<string> GetVariables()
        {
            var variables = new List<string>();
            variables.AddRange(Condition.GetVariables());
            foreach (var command in IfBlockCommands)
            {
                variables.AddRange(command.GetVariables());
            }
            foreach (var command in ElseBlockCommands)
            {
                variables.AddRange(command.GetVariables());
            }
            return variables;
        }
    }
}
