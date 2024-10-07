using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using System.Collections.Generic;

public interface ICommandLifetimeService
{
    List<IDropletCommand> CommandsInAction { get; }
    void StoreCommand(IDropletCommand dropletCommand);
    bool CanExecuteCommand(IDropletCommand dropletCommand);
    void RemoveCommand(IDropletCommand dropletCommand);
}