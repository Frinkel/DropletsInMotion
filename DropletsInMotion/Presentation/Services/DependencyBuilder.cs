using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Presentation.Services
{
    public class DependencyBuilder : IDependencyBuilder
    {
        public DependencyBuilder()
        {

        }

        public DependencyGraph Build(List<ICommand> commands)
        {
            List<DependencyNode> nodes = new List<DependencyNode>();

            // Create nodes for each command in the list
            for (int index = 0; index < commands.Count; index++)
            {
                var node = new DependencyNode(index, commands[index]);
                nodes.Add(node);
            }

            // Establish dependencies between nodes
            BuildDependencies(nodes);

            DependencyGraph graph = new DependencyGraph(nodes);
            return graph;
        }

        private void BuildDependencies(List<DependencyNode> nodes)
        {
            DependencyNode lastWaitNode = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                funsds(i, nodes, lastWaitNode);
            }
        }

        private void funsds(int  i, List<DependencyNode> nodes, DependencyNode lastWaitNode)
        {
            var currentNode = nodes[i];
            var currentInputs = new List<string>();
            if (currentNode.Command is IDropletCommand)
            {
                currentInputs = (currentNode.Command as IDropletCommand).GetInputDroplets();
            }
            //var currentInputs = currentNode.Command.GetInputDroplets();

            for (int j = 0; j < i; j++)
            {
                var potentialDependency = nodes[j];
                //var potentialOutputs = potentialDependency.Command.GetOutputDroplets();
                var potentialOutputs = new List<string>();
                if (potentialDependency.Command is IDropletCommand)
                {
                    potentialOutputs = (potentialDependency.Command as IDropletCommand).GetOutputDroplets();
                }


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

        private bool IsWaitCommand(ICommand command)
        {
            return command is Wait || command is WaitForUserInput;
        }
    }
}
