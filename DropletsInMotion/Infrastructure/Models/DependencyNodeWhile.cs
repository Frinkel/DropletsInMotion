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

        public void ResetBody()
        {
            Console.WriteLine("Resetting body");
            foreach (var node in Body.GetAllNodes())
            {
                node.IsExecuted = false;
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

        public List<IDependencyNode> getExecutableNodes()
        {
            if(Body.GetAllNodes().All(node => node.IsExecuted))
            {
                return new List<IDependencyNode>() { this };
            }
            return new List<IDependencyNode>(Body.GetExecutableNodes());

        }

        public bool CanExecute()
        {
            //bool canExecute = Dependencies.All(dependency => dependency.IsExecuted);
                

            //if (canExecute && Body.GetAllNodes().All(node => node.IsExecuted))
            //{

            //}
            Console.WriteLine($"CanExecute: {Dependencies.All(dependency => dependency.IsExecuted)} and is {IsExecuted}");

            return Dependencies.All(dependency => dependency.IsExecuted);
        }

        public override string ToString()
        {
            return $"Node {NodeId}: {Command.ToString()} (Executed: {IsExecuted})";
        }
    }
}