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
                } else if (commands[index] is IfCommand)
                {
                    IfCommand ifCommand = (IfCommand)commands[index];
                    DependencyGraph thenBody = Build(ifCommand.IfBlockCommands);
                    DependencyGraph elseBody = Build(ifCommand.ElseBlockCommands);
                    nodes.Add(new DependencyNodeIf(nodeId, commands[index], thenBody, elseBody));
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

            var currentInputVariables = currentNode.Command.GetInputVariables();
            var currentOutputVariables = currentNode.Command.GetOutputVariables();


            for (int previousIndex = 0; previousIndex < currentIndex; previousIndex++)
            {
                var potentialDependency = nodes[previousIndex];

                var potentialOutputVariables = potentialDependency.Command.GetOutputVariables();
                if (currentInputVariables.Intersect(potentialOutputVariables).Any())
                {
                    currentNode.AddDependency(potentialDependency);
                }

                var potentialInputVariables = potentialDependency.Command.GetInputVariables();
                if (currentOutputVariables.Intersect(potentialInputVariables).Any())
                {
                    currentNode.AddDependency(potentialDependency);
                }

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
        private bool IsWaitCommand(ICommand command)
        {
            return command is Wait || command is WaitForUserInput;
        }


        public void CleanDependencies(DependencyGraph graph)
        {
            foreach (var node in graph.GetAllNodes())
            {
                var directDependencies = node.Dependencies.ToList();

                var uniqueDependencies = directDependencies
                    .GroupBy(dep => dep.NodeId)
                    .Select(group => group.First())
                    .ToList();

                node.Dependencies.Clear();
                node.Dependencies.AddRange(uniqueDependencies);

                var transitiveDependencies = new HashSet<IDependencyNode>();

                foreach (var dependency in uniqueDependencies)
                {
                    CollectTransitiveDependencies(dependency, transitiveDependencies);
                }

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
