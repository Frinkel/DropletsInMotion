using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Translation.Services;

public interface IPlatformService
{
    Electrode[][] Board { get; set; }
    void PrintBoard();
    void Load();


}