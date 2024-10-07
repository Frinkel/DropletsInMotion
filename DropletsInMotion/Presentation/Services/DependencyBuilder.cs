using System;
using System.Collections.Generic;
using System.Linq;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Presentation.Services
{
    public class DependencyBuilder : IDependencyBuilder
    {
        private int nodeId = 0;
        public DependencyGraph Build(List<ICommand> commands)
        {
            List<IDependencyNode> nodes = CreateNodes(commands);
            BuildDependencies(nodes);
            var graph = new DependencyGraph(nodes);
            CleanDependencies(graph);
            return graph;
        }

        private List<IDependencyNode> CreateNodes(List<ICommand> commands)
        {
            var nodes = new List<IDependencyNode>();
            for (int index = 0; index < commands.Count; index++)
            {
                if (commands[index] is WhileCommand)
                {
                    WhileCommand whileCommand = (WhileCommand)commands[index];
                    DependencyGraph body = Build(whileCommand.Commands);
                    nodes.Add(new DependencyNodeWhile(nodeId, commands[index], body));
                    body.GenerateDotFile();
                } else if (commands[index] is IfCommand)
                {
                    //IfCommand ifCommand = (IfCommand)commands[index];
                    //DependencyGraph body = Build(ifCommand.Commands);
                    //nodes.Add(new DependencyNodeIf(nodeId, commands[index], body);
                }
                else
                {
                    nodes.Add(new DependencyNode(nodeId, commands[index]));

                }
                nodeId++;
            }
            return nodes;
        }

        private void BuildDependencies(List<IDependencyNode> nodes)
        {
            IDependencyNode lastWaitNode = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                var currentNode = nodes[i];
                HandleDependenciesForCurrentNode(i, nodes, ref lastWaitNode);
            }
        }

        private void HandleDependenciesForCurrentNode(int currentIndex, List<IDependencyNode> nodes, ref IDependencyNode lastWaitNode)
        {
            var currentNode = nodes[currentIndex];
            var currentInputsDroplets = GetDropletInputs(currentNode);
            var currentVariables = currentNode.Command.GetVariables();

            for (int previousIndex = 0; previousIndex < currentIndex; previousIndex++)
            {
                var potentialDependency = nodes[previousIndex];
                HandleDropletDependencies(currentNode, potentialDependency, currentInputsDroplets);
                HandleVariableDependencies(currentNode, potentialDependency, currentVariables);
                HandleAssignDependencies(currentNode, potentialDependency);

                if (IsWaitCommand(currentNode.Command))
                {
                    currentNode.AddDependency(potentialDependency);
                }
            }

            if (lastWaitNode != null)
            {
                currentNode.AddDependency(lastWaitNode);
            }

            if (IsWaitCommand(currentNode.Command))
            {
                lastWaitNode = currentNode;
            }
        }

        private void HandleDropletDependencies(IDependencyNode currentNode, IDependencyNode potentialDependency, List<string> currentInputsDroplets)
        {
            var potentialOutputsDroplets = GetDropletOutputs(potentialDependency);
            if (currentInputsDroplets.Intersect(potentialOutputsDroplets).Any())
            {
                currentNode.AddDependency(potentialDependency);
            }
        }

        private void HandleVariableDependencies(IDependencyNode currentNode, IDependencyNode potentialDependency, List<string> currentVariables)
        {
            if (potentialDependency.Command is Assign assignCommand)
            {
                if (currentVariables.Contains(assignCommand.VariableName))
                {
                    currentNode.AddDependency(potentialDependency);
                }
            }
        }

        private void HandleAssignDependencies(IDependencyNode currentNode, IDependencyNode potentialDependency)
        {
            if (currentNode.Command is Assign)
            {
                var potentialVariables = potentialDependency.Command.GetVariables();
                var currentVariables = currentNode.Command.GetVariables();
                if (potentialVariables.Intersect(currentVariables).Any())
                {
                    currentNode.AddDependency(potentialDependency);
                }
            }
        }

        private List<string> GetDropletInputs(IDependencyNode node)
        {
            if (node.Command is IDropletCommand dropletCommand)
            {
                return dropletCommand.GetInputDroplets();
            }
            return new List<string>();
        }

        private List<string> GetDropletOutputs(IDependencyNode node)
        {
            if (node.Command is IDropletCommand dropletCommand)
            {
                return dropletCommand.GetOutputDroplets();
            }
            return new List<string>();
        }

        private bool IsWaitCommand(ICommand command)
        {
            return command is Wait || command is WaitForUserInput;
        }


        public void CleanDependencies(DependencyGraph graph)
        {
            foreach (var node in graph.GetAllNodes())
            {
                // Get all dependencies of this node
                var directDependencies = node.Dependencies.ToList();

                // Track dependencies that are already covered transitively
                var transitiveDependencies = new HashSet<IDependencyNode>();

                foreach (var dependency in directDependencies)
                {
                    // Collect all dependencies of the current dependency
                    CollectTransitiveDependencies(dependency, transitiveDependencies);
                }

                // Remove any dependencies that are already transitively covered
                foreach (var redundantDependency in transitiveDependencies)
                {
                    if (node.Dependencies.Contains(redundantDependency))
                    {
                        node.Dependencies.Remove(redundantDependency);
                    }
                }
            }
        }

        private void CollectTransitiveDependencies(IDependencyNode node, HashSet<IDependencyNode> collectedDependencies)
        {
            foreach (var dependency in node.Dependencies)
            {
                if (!collectedDependencies.Contains(dependency))
                {
                    collectedDependencies.Add(dependency);
                    CollectTransitiveDependencies(dependency, collectedDependencies); // Recursive call
                }
            }
        }
    }
}
