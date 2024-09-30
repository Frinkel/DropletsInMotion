using DropletsInMotion.Infrastructure.Models.Commands;

public class CommandManager
{
    public List<ICommand> CommandsInAction { get; private set; } = new List<ICommand>();

    public CommandManager() { }

    public void StoreCommand(ICommand command)
    {
        CommandsInAction.Add(command);
    }

    public bool CanExecuteCommand(ICommand command)
    {
        return !CommandsInAction.Any(existingCommand => CommandEquals(existingCommand, command));
    }

    public void RemoveCommand(ICommand command)
    {
        CommandsInAction.RemoveAll(existingCommand => CommandEquals(existingCommand, command));
    }

    private bool CommandEquals(ICommand command1, ICommand command2)
    {
        var inputComparison = command1.GetInputDroplets().OrderBy(i => i).SequenceEqual(command2.GetInputDroplets().OrderBy(i => i));
        var outputComparison = command1.GetOutputDroplets().OrderBy(o => o).SequenceEqual(command2.GetOutputDroplets().OrderBy(o => o));

        return inputComparison && outputComparison;
    }
}