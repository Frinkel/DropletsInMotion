using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Models
{
    public class DependencyNode : IDependencyNode
    {
        public int NodeId { get; }
        public ICommand Command { get; }
        public bool IsExecuted { get; set; }
        public List<IDependencyNode> Dependencies { get; }

        public DependencyNode(int nodeId, ICommand command)
        {
            NodeId = nodeId;
            Command = command;
            IsExecuted = false;
            Dependencies = new List<IDependencyNode>();
        }

        public void MarkAsExecuted()
        {
            IsExecuted = true;
        }

        public void AddDependency(IDependencyNode dependency)
        {
            Dependencies.Add(dependency);
        }

        public List<IDependencyNode> GetExecutableNodes()
        {
            return new List<IDependencyNode>() {this};
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
        }
    }
}