using System.Text;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Application.ExecutionEngine.Models
{
    public class DependencyGraph
    {
        private readonly List<DependencyNode> nodes;

        public DependencyGraph(List<ICommand> commands)
        {
            nodes = new List<DependencyNode>();

            // Create nodes for each command in the list
            for (int index = 0; index < commands.Count; index++)
            {
                var node = new DependencyNode(index, commands[index]);
                nodes.Add(node);
            }

            // Establish dependencies between nodes
            BuildDependencies();
        }

        private void BuildDependencies()
        {
            DependencyNode lastWaitNode = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                var currentNode = nodes[i];
                var currentInputs = currentNode.Command.GetInputDroplets();

                for (int j = 0; j < i; j++)
                {
                    var potentialDependency = nodes[j];
                    var potentialOutputs = potentialDependency.Command.GetOutputDroplets();

                    if (currentInputs.Intersect(potentialOutputs).Any())
                    {
                        currentNode.AddDependency(potentialDependency);
                    }

                    if (IsWaitCommand(currentNode.Command))
                    {
                        currentNode.AddDependency(potentialDependency);
                    }

                    if (lastWaitNode != null)
                    {
                        currentNode.AddDependency(lastWaitNode);
                    }
                }

                if (IsWaitCommand(currentNode.Command))
                {
                    lastWaitNode = currentNode;
                }
            }
        }

        private bool IsWaitCommand(ICommand command)
        {
            return command is Wait || command is WaitForUserInput;
        }


        public List<DependencyNode> GetExecutableNodes()
        {
            return nodes.Where(n => !n.IsExecuted && n.CanExecute()).ToList();
        }

        public void MarkNodeAsExecuted(int nodeId)
        {
            var node = nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                node.MarkAsExecuted();
            }
            // remove dependency for all nodes? for speedup or no gain?
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var node in nodes)
            {
                sb.AppendLine(node.ToString());
            }
            return sb.ToString();
        }

        public void PrintGraph()
        {
            foreach (var node in nodes)
            {
                Console.WriteLine($"Node {node.NodeId}: {node.Command}");

                if (node.Dependencies.Count > 0)
                {
                    Console.Write("  Dependencies: ");
                    foreach (var dependency in node.Dependencies)
                    {
                        Console.Write($"{dependency.NodeId} ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("  Dependencies: None");
                }
            }
        }

        public void GenerateDotFile()
        {
            Console.WriteLine("digraph DependencyGraph {");
            foreach (var node in nodes)
            {
                // Define the node
                Console.WriteLine($"  Node{node.NodeId} [label=\"{node.Command}\"];");

                // Define the edges (dependencies)
                foreach (var dependency in node.Dependencies)
                {
                    Console.WriteLine($"  Node{dependency.NodeId} -> Node{node.NodeId};");
                }
            }
            Console.WriteLine("}");

        }

        
    }


}
