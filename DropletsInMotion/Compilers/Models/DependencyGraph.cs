using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public void updateExecutedNodes(List<DependencyNode> nodes, Dictionary<string, Droplet> droplets)
        {
            foreach (DependencyNode node in nodes)
            {
                switch (node.Command)
                {
                    case Move moveCommand:
                        if (droplets.TryGetValue(moveCommand.DropletName, out var moveDroplet))
                        {
                            if (moveDroplet.PositionX == moveCommand.PositionX &&
                                moveDroplet.PositionY == moveCommand.PositionY)
                            {
                                Console.WriteLine("REMOVEMOVE");
                                MarkNodeAsExecuted(node.NodeId);
                            }
                        }
                        break;
                    case Merge mergeCommand:
                        if (droplets.TryGetValue(mergeCommand.OutputName, out var mergeDroplet) && 
                            (mergeCommand.OutputName == mergeCommand.InputName1 || !droplets.ContainsKey(mergeCommand.InputName1)) &&
                            (mergeCommand.OutputName == mergeCommand.InputName2 || !droplets.ContainsKey(mergeCommand.InputName2)) )
                        {
                            if (mergeDroplet.PositionX == mergeCommand.PositionX &&
                                mergeDroplet.PositionY == mergeCommand.PositionY)
                            {
                                Console.WriteLine("REMOVE MERGRE");
                                MarkNodeAsExecuted(node.NodeId);
                            }
                        }

                        break;
                    case SplitByRatio splitByRatio:
                        if (droplets.TryGetValue(splitByRatio.OutputName1, out var splitDroplet1) &&
                            droplets.TryGetValue(splitByRatio.OutputName2, out var splitDroplet2) &&
                            (splitByRatio.OutputName1 == splitByRatio.InputName || !droplets.ContainsKey(splitByRatio.InputName)) &&
                            (splitByRatio.OutputName2 == splitByRatio.InputName || !droplets.ContainsKey(splitByRatio.InputName)))
                        {
                            if (splitDroplet1.PositionX == splitByRatio.PositionX1 &&
                                splitDroplet1.PositionY == splitByRatio.PositionY1 &&
                                splitDroplet2.PositionX == splitByRatio.PositionX2 &&
                                splitDroplet2.PositionY == splitByRatio.PositionY2)
                            {
                                Console.WriteLine("REMOVE SPLIT!");
                                MarkNodeAsExecuted(node.NodeId);
                            }
                        }

                        break;
                    case SplitByVolume splitByVolume:
                        if (droplets.TryGetValue(splitByVolume.OutputName1, out var splitDroplet1v) &&
                            droplets.TryGetValue(splitByVolume.OutputName2, out var splitDroplet2v) &&
                            (splitByVolume.OutputName1 == splitByVolume.InputName || !droplets.ContainsKey(splitByVolume.InputName)) &&
                            (splitByVolume.OutputName2 == splitByVolume.InputName || !droplets.ContainsKey(splitByVolume.InputName)))
                        {
                            if (splitDroplet1v.PositionX == splitByVolume.PositionX1 &&
                                splitDroplet1v.PositionY == splitByVolume.PositionY1 &&
                                splitDroplet2v.PositionX == splitByVolume.PositionX2 &&
                                splitDroplet2v.PositionY == splitByVolume.PositionY2)
                            {
                                MarkNodeAsExecuted(node.NodeId);
                            }
                        }

                        break;
                    case Mix mixCommand:
                        throw new NotSupportedException($"Command type {node.Command.GetType()} is not supported.");


                    default:
                        throw new NotSupportedException($"Command type {node.Command.GetType()} is not supported.");
                }
            }

        }
    }


}
