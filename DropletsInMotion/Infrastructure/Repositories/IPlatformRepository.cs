using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface IPlatformRepository
{
    Electrode[][] Board { get; set; }
    double MinimumMovementVolume { get; set; }
    double MaximumMovementVolume { get; set; }
}