﻿using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class WhileCommand : ICommand
    {
        public BooleanExpression Condition { get; }
        public List<ICommand> Commands { get; }
        public bool Evaluation { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public WhileCommand(BooleanExpression condition, List<ICommand> commands)
        {
            Condition = condition;
            Commands = commands;
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            Evaluation = Condition.Evaluate(variableValues);
        }

        public override string ToString()
        {
            return $"While({Condition})";
        }

        public List<string> GetInputVariables()
        {
            var variables = new List<string>();
            variables.AddRange(Condition.GetVariables());
            foreach (var command in Commands)
            {
                variables.AddRange(command.GetInputVariables());
            }
            return variables;
        }

        public List<string> GetOutputVariables()
        {
            var variables = new List<string>();
            foreach (var command in Commands)
            {
                variables.AddRange(command.GetOutputVariables());
            }
            return variables;
        }

    }
}
