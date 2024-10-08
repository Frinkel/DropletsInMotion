using System.Text;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Infrastructure.Models
{
    public class DependencyGraph
    {
        private readonly List<IDependencyNode> _nodes;

        public DependencyGraph(List<IDependencyNode> nodes)
        {
            _nodes = nodes;
        }

        public List<IDependencyNode> GetExecutableNodes()
        {

            // Aggregate executable nodes from each DependencyNode using their getExecutableNodes method
            return _nodes
                .Where(n => !n.IsExecuted && n.CanExecute())
                .SelectMany(n => n.getExecutableNodes())
                .ToList();
        }
        
        public List<IDependencyNode> GetAllNodes()
        {
            return _nodes;
        }

        public void MarkNodeAsExecuted(int nodeId)
        {
            var node = _nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                node.MarkAsExecuted();
            }
            // remove dependency for all _nodes? for speedup or no gain?
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var node in _nodes)
            {
                sb.AppendLine(node.ToString());
            }
            return sb.ToString();
        }

        public void PrintGraph()
        {
            foreach (var node in _nodes)
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
            foreach (var node in _nodes)
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
