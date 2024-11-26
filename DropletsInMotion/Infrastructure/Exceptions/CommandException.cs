using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class CommandException : Exception
    {
        public ICommand Command { get; }

        public CommandException(string message, ICommand command) : base(message)
        {
            Command = command;
        }

        public CommandException(string message, ICommand command, Exception innerException) : base(message, innerException)
        {
            Command = command;
        }
    }
}
