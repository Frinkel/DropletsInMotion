using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Presentation;

public interface ITranslator
{
    Electrode[][] Board { get; set; }
    List<ICommand> Commands { get; set; }
    DependencyGraph DependencyGraph { get; set; }
    void Translate();
    void Load();
}