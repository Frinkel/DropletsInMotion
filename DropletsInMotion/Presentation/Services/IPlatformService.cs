using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Presentation.Services;

public interface IPlatformService
{
    Electrode[][] Board { get; set; }
    void LoadBoardFromJson(string jsonFilePath);
    void PrintBoard();
}