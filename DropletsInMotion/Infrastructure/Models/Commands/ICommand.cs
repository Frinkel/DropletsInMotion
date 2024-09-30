namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public interface ICommand
    {
        List<string> GetInputDroplets();
        List<string> GetOutputDroplets();
    }
}
