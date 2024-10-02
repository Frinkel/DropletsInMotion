using System;
using System.Collections.Generic;
using System.Linq;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Presentation.Services
{
    public class DependencyBuilder : IDependencyBuilder
    {
        public DependencyGraph Build(List<ICommand> commands)
        {
            List<DependencyNode> nodes = CreateNodes(commands);
            BuildDependencies(nodes);
            var graph = new DependencyGraph(nodes);
            CleanDependencies(graph);
            return graph;
        }

        private List<DependencyNode> CreateNodes(List<ICommand> commands)
        {
            var nodes = new List<DependencyNode>();
            for (int index = 0; index < commands.Count; index++)
            {
                nodes.Add(new DependencyNode(index, commands[index]));
            }
            return nodes;
        }

        private void BuildDependencies(List<DependencyNode> nodes)
        {
            DependencyNode lastWaitNode = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                var currentNode = nodes[i];
                HandleDependenciesForCurrentNode(i, nodes, ref lastWaitNode);
            }
        }

        private void HandleDependenciesForCurrentNode(int currentIndex, List<DependencyNode> nodes, ref DependencyNode lastWaitNode)
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

        private void HandleDropletDependencies(DependencyNode currentNode, DependencyNode potentialDependency, List<string> currentInputsDroplets)
        {
            var potentialOutputsDroplets = GetDropletOutputs(potentialDependency);
            if (currentInputsDroplets.Intersect(potentialOutputsDroplets).Any())
            {
                currentNode.AddDependency(potentialDependency);
            }
        }

        private void HandleVariableDependencies(DependencyNode currentNode, DependencyNode potentialDependency, List<string> currentVariables)
        {
            if (potentialDependency.Command is Assign assignCommand)
            {
                if (currentVariables.Contains(assignCommand.VariableName))
                {
                    currentNode.AddDependency(potentialDependency);
                }
            }
        }

        private void HandleAssignDependencies(DependencyNode currentNode, DependencyNode potentialDependency)
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

        private List<string> GetDropletInputs(DependencyNode node)
        {
            if (node.Command is IDropletCommand dropletCommand)
            {
                return dropletCommand.GetInputDroplets();
            }
            return new List<string>();
        }

        private List<string> GetDropletOutputs(DependencyNode node)
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
                var transitiveDependencies = new HashSet<DependencyNode>();

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

        private void CollectTransitiveDependencies(DependencyNode node, HashSet<DependencyNode> collectedDependencies)
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
