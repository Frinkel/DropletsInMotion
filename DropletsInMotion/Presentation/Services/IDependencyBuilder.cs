using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Presentation.Services
{
    public interface IDependencyBuilder
    {
        DependencyGraph Build(List<ICommand> commands);
    }
}