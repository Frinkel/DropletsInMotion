using DropletsInMotion.Domain;

namespace DropletsInMotion.Compilers.Models
{
    public class DependencyNode
    {
        public int NodeId { get; }
        public ICommand Command { get; }
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
            // A node can be executed if all its dependencies have been executed
            return Dependencies.All(dependency => dependency.IsExecuted);
        }

        public override string ToString()
        {
            return $"Node {NodeId}: {Command.ToString()} (Executed: {IsExecuted})";
        }
    }

}
