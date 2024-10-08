using DropletsInMotion.Infrastructure.Models.Commands;
using System;

namespace DropletsInMotion.Infrastructure.Models
{
    public class DependencyNodeIf : IDependencyNode
    {
        public int NodeId { get; }
        public ICommand Command { get; }
        public bool IsExecuted { get; set; }
        public List<IDependencyNode> Dependencies { get; }
        public DependencyGraph ThenBody { get; set; }
        public DependencyGraph ElseBody { get; set; }
        //public bool ConditionIsTrue { get; set; }


        public DependencyNodeIf(int nodeId, ICommand command, DependencyGraph thenBody, DependencyGraph elseBody)
        {
            NodeId = nodeId;
            Command = command;
            ThenBody = thenBody;
            ElseBody = elseBody;
            //SetBodyExecuted();
            ResetBody();
            IsExecuted = false;
            Dependencies = new List<IDependencyNode>();
        }

        public void ResetBody()
        {
            foreach (var node in ThenBody.GetAllNodes())
            {
                node.IsExecuted = false;
            }
            foreach (var node in ElseBody.GetAllNodes())
            {
                node.IsExecuted = false;
            }
        }

        private void SetBodyExecuted()
        {
            foreach (var node in ThenBody.GetAllNodes())
            {
                node.IsExecuted = true;
            }
            foreach (var node in ElseBody.GetAllNodes())
            {
                node.IsExecuted = true;
            }
        }

        public void MarkAsExecuted()
        {
            IsExecuted = true;
        }

        public void AddDependency(IDependencyNode dependency)
        {
            Dependencies.Add(dependency);
        }

        public List<IDependencyNode> getExecutableNodes()
        {
            IfCommand ifCommand = (IfCommand)Command;


            if (!ifCommand.HasBeenEvaluated)
            {
                return new List<IDependencyNode>() { this };
            }

            if (IsComplete())
            {
                Console.WriteLine("IS COMPLEET!");
                return new List<IDependencyNode>() { this };
            }


            if (ifCommand.Evaluation)
            {
                return new List<IDependencyNode>(ThenBody.GetExecutableNodes());
            }
            return new List<IDependencyNode>(ElseBody.GetExecutableNodes());
        }

        public bool IsComplete()
        {
            IfCommand ifCommand = (IfCommand)Command;
            if (ifCommand.Evaluation)
            {
                return ThenBody.GetAllNodes().All(n => n.IsExecuted);
            }

            return ElseBody.GetAllNodes().All(n => n.IsExecuted);
        }

        public bool CanExecute()
        {
            return Dependencies.All(dependency => dependency.IsExecuted);
        }

        public override string ToString()
        {
            return $"Node {NodeId}: {Command.ToString()} (Executed: {IsExecuted})";
        }

        public void Reset()
        {
            IsExecuted = false;
            IfCommand ifCommand = (IfCommand)Command;
            ifCommand.HasBeenEvaluated = false;

            foreach (var node in ThenBody.GetAllNodes())
            {
                node.Reset();
            }
            foreach (var node in ElseBody.GetAllNodes())
            {
                node.Reset();
            }
        }
    }
}