using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Presentation.Services
{
    public interface IDependencyBuilder
    {
        DependencyGraph Build(List<ICommand> commands);
    }
}