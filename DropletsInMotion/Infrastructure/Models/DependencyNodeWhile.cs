using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Models
{
    public class DependencyNodeWhile : IDependencyNode
    {
        public int NodeId { get; }
        public ICommand Command { get; }
        public bool IsExecuted { get; set; }
        public List<IDependencyNode> Dependencies { get; }

        public DependencyGraph Body { get; set; }

        public DependencyNodeWhile(int nodeId, ICommand command, DependencyGraph body)
        {
            NodeId = nodeId;
            Command = command;
            Body = body;
            SetBodyExecuted();
            IsExecuted = false;
            Dependencies = new List<IDependencyNode>();
        }

        public void Reset()
        {
            IsExecuted = false;
            foreach (var node in Body.GetAllNodes())
            {
                node.Reset();
            }
        }

        private void SetBodyExecuted()
        {
            foreach (var node in Body.GetAllNodes())
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

        public List<IDependencyNode> GetExecutableNodes()
        {
            if(Body.GetAllNodes().All(node => node.IsExecuted))
            {
                return new List<IDependencyNode>() { this };
            }
            return new List<IDependencyNode>(Body.GetExecutableNodes());

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