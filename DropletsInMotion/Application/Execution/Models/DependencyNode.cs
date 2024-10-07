using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Application.ExecutionEngine.Models
{
    public class DependencyNode
    {
        public int NodeId { get; }
        public ICommand Command { get; }  // Changed to ICommand
        public bool IsExecuted { get; private set; }
        public List<DependencyNode> Dependencies { get; }

        public DependencyNode(int nodeId, ICommand command)
        {
            NodeId = nodeId;
            Command = command;
            IsExecuted = false;
            Dependencies = new List<DependencyNode>();
        }

        public void MarkAsExecuted()
        {
            IsExecuted = true;
        }

        public void AddDependency(DependencyNode dependency)
        {
            Dependencies.Add(dependency);
        }

        public bool CanExecute()
        {
            return Dependencies.All(dependency => dependency.IsExecuted);
        }

        public override string ToString()
        {
            return $"Node {NodeId}: {Command.ToString()} (Executed: {IsExecuted})";
        }
    }
}