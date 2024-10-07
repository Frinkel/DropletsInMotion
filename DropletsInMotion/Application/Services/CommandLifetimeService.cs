using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
namespace DropletsInMotion.Application.Services
{
    public class CommandLifetimeService : ICommandLifetimeService
    {
        public List<IDropletCommand> CommandsInAction { get; private set; } = new List<IDropletCommand>();

        public CommandLifetimeService() { }

        public void StoreCommand(IDropletCommand dropletCommand)
        {
            CommandsInAction.Add(dropletCommand);
        }

        public bool CanExecuteCommand(IDropletCommand dropletCommand)
        {
            return !CommandsInAction.Any(existingCommand => CommandEquals(existingCommand, dropletCommand));
        }

        public void RemoveCommand(IDropletCommand dropletCommand)
        {
            CommandsInAction.RemoveAll(existingCommand => CommandEquals(existingCommand, dropletCommand));
        }

        private bool CommandEquals(IDropletCommand command1, IDropletCommand command2)
        {
            var inputComparison = command1.GetInputDroplets().OrderBy(i => i).SequenceEqual(command2.GetInputDroplets().OrderBy(i => i));
            var outputComparison = command1.GetOutputDroplets().OrderBy(o => o).SequenceEqual(command2.GetOutputDroplets().OrderBy(o => o));

            return inputComparison && outputComparison;
        }
    }
}