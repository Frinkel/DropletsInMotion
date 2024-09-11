using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Domain;

namespace DropletsInMotion.Compilers.Models
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
    }


}
