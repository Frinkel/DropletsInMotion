using System.Text;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Models.Commands;

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

        public void GenerateDotFile(bool isSubgraph = false)
        {
            // Only print the "digraph" definition at the top level, not for subgraphs
            if (!isSubgraph)
            {
                Console.WriteLine("digraph DependencyGraph {");
            }

            foreach (var node in _nodes)
            {
                Console.WriteLine($"  Node{node.NodeId} [label=\"{node.Command}\"];");

                foreach (var dependency in node.Dependencies)
                {
                    Console.WriteLine($"  Node{dependency.NodeId} -> Node{node.NodeId};");
                }

                if (node is DependencyNodeWhile whileNode && whileNode.Body != null)
                {
                    Console.WriteLine($"  subgraph cluster_while_{node.NodeId} {{");
                    Console.WriteLine($"    label = \"Subgraph for While Node: {node.Command}\";");
                    whileNode.Body.GenerateDotFile(true);  
                    Console.WriteLine("  }");
                }

                if (node is DependencyNodeIf ifNode && ifNode.ThenBody != null)
                {
                    Console.WriteLine($"  subgraph cluster_if_then_{node.NodeId} {{");
                    Console.WriteLine($"    label = \"Subgraph for IF Node THEN path: {node.Command}\";");
                    ifNode.ThenBody.GenerateDotFile(true); 
                    Console.WriteLine("  }");

                    if (ifNode.ElseBody != null)
                    {
                        Console.WriteLine($"  subgraph cluster_if_else_{node.NodeId} {{");
                        Console.WriteLine($"    label = \"Subgraph for IF Node ELSE path: {node.Command}\";");
                        ifNode.ElseBody.GenerateDotFile(true); 
                        Console.WriteLine("  }");
                    }

                }
            }

            if (!isSubgraph)
            {
                Console.WriteLine("}");
            }
        }

    }


}
