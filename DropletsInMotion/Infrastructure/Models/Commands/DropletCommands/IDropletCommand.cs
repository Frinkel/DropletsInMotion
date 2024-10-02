namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public interface IDropletCommand : ICommand
    {
        List<string> GetInputDroplets();
        List<string> GetOutputDroplets();

    }
}
