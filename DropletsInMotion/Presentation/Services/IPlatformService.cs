using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Presentation.Services;

public interface IPlatformService
{
    Electrode[][] Board { get; set; }
    void LoadBoardFromJson(string jsonFilePath);
    void PrintBoard();
    public void LoadPlatformInformation();
}