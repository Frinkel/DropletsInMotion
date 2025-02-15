﻿using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Application.Services
{
    public class DependencyService : IDependencyService
    {
        IStoreService _storeService;
        ICommandLifetimeService _commandLifetimeService;

        public DependencyService(IStoreService storeService, ICommandLifetimeService commandLifetimeService)
        {
            _storeService = storeService;
            _commandLifetimeService = commandLifetimeService;
        }

        public void UpdateExecutedNodes(List<IDependencyNode> nodes, Dictionary<string, Agent> agents, double currentTime)
        {
            foreach (IDependencyNode node in nodes)
            {
                switch (node.Command)
                {
                    case Move moveCommand:
                        if (agents.TryGetValue(moveCommand.DropletName, out var moveDroplet))
                        {
                            if (moveDroplet.PositionX == moveCommand.PositionX &&
                                moveDroplet.PositionY == moveCommand.PositionY)
                            {
                                node.MarkAsExecuted();
                            }
                        }
                        break;

                    case Merge mergeCommand:
                        if (agents.TryGetValue(mergeCommand.OutputName, out var mergeDroplet) &&
                            (mergeCommand.OutputName == mergeCommand.InputName1 || !agents.ContainsKey(mergeCommand.InputName1)) &&
                            (mergeCommand.OutputName == mergeCommand.InputName2 || !agents.ContainsKey(mergeCommand.InputName2)))
                        {
                            if (mergeDroplet.PositionX == mergeCommand.PositionX &&
                                mergeDroplet.PositionY == mergeCommand.PositionY)
                            {
                                _commandLifetimeService.RemoveCommand((IDropletCommand) node.Command);
                                node.MarkAsExecuted();
                            }
                        }

                        break;

                    case SplitByRatio splitByRatio:
                        if (agents.TryGetValue(splitByRatio.OutputName1, out var splitDroplet1) &&
                            agents.TryGetValue(splitByRatio.OutputName2, out var splitDroplet2) &&
                            (splitByRatio.OutputName1 == splitByRatio.InputName || splitByRatio.OutputName2 == splitByRatio.InputName ||
                            !agents.ContainsKey(splitByRatio.InputName)))
                        {
                            if (splitDroplet1.PositionX == splitByRatio.PositionX1 &&
                                splitDroplet1.PositionY == splitByRatio.PositionY1 &&
                                splitDroplet2.PositionX == splitByRatio.PositionX2 &&
                                splitDroplet2.PositionY == splitByRatio.PositionY2)
                            {

                                _commandLifetimeService.RemoveCommand((IDropletCommand)node.Command);
                                node.MarkAsExecuted();
                            }
                        }

                        break;

                    case SplitByVolume splitByVolume:
                        if (agents.TryGetValue(splitByVolume.OutputName1, out var splitDroplet1v) &&
                            agents.TryGetValue(splitByVolume.OutputName2, out var splitDroplet2v) &&
                            (splitByVolume.OutputName1 == splitByVolume.InputName || splitByVolume.OutputName2 == splitByVolume.InputName ||
                             !agents.ContainsKey(splitByVolume.InputName)))
                        {
                            if (splitDroplet1v.PositionX == splitByVolume.PositionX1 &&
                                splitDroplet1v.PositionY == splitByVolume.PositionY1 &&
                                splitDroplet2v.PositionX == splitByVolume.PositionX2 &&
                                splitDroplet2v.PositionY == splitByVolume.PositionY2)
                            {
                                _commandLifetimeService.RemoveCommand((IDropletCommand)node.Command);
                                node.MarkAsExecuted();
                            }
                        }

                        break;

                    case Store storeCommand:
                        if (_storeService.IsStoreComplete(storeCommand.DropletName, currentTime))
                        {
                            node.MarkAsExecuted();
                        }
                        break;

                    case WaitForUserInput command:
                        node.MarkAsExecuted();
                        break;

                    case Wait command:
                        node.MarkAsExecuted();
                        break;

                    case Mix mixCommand:
                        if (_storeService.IsStoreComplete(mixCommand.DropletName, currentTime))
                        {
                            node.MarkAsExecuted();
                        }
                        break;

                    case WhileCommand whileCommand:
                        DependencyNodeWhile dependencyNodeWhile = (DependencyNodeWhile)node;
                        if (dependencyNodeWhile.Body.GetAllNodes().All(n => n.IsExecuted))
                        {
                            if (!whileCommand.Evaluation)
                            {
                                node.MarkAsExecuted();
                            }
                            else
                            {
                                dependencyNodeWhile.Reset();
                            }
                            
                        }
                        break;

                    case IfCommand ifCommand:
                        DependencyNodeIf dependencyNodeIf = (DependencyNodeIf)node;
                        if (dependencyNodeIf.IsComplete())
                        {
                            node.MarkAsExecuted();
                        }
                        break;

                    case SensorCommand sensorCommand:
                        if (!_commandLifetimeService.CanExecuteCommand(sensorCommand))
                        {
                            _commandLifetimeService.RemoveCommand(sensorCommand);
                            node.MarkAsExecuted();
                        }
                        break;

                    case Waste wasteCommand:
                        if (!agents.ContainsKey(wasteCommand.DropletName))
                        {
                            node.MarkAsExecuted();
                        }
                        break;

                    case Dispense dispenseCommand:
                        if(dispenseCommand.Completed)
                        {
                            node.MarkAsExecuted();
                        }
                        break;

                    default:
                        node.MarkAsExecuted();
                        break;
                }
            }
        }
    }
}
