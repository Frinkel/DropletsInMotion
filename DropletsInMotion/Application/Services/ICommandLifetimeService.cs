using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

public interface ICommandLifetimeService
{
    List<IDropletCommand> CommandsInAction { get; }
    void StoreCommand(IDropletCommand dropletCommand);
    bool CanExecuteCommand(IDropletCommand dropletCommand);
    void RemoveCommand(IDropletCommand dropletCommand);
}