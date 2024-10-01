using DropletsInMotion.Infrastructure.Models.Commands;
using System.Collections.Generic;

public interface ICommandLifetimeService
{
    List<ICommand> CommandsInAction { get; }
    void StoreCommand(ICommand command);
    bool CanExecuteCommand(ICommand command);
    void RemoveCommand(ICommand command);
}