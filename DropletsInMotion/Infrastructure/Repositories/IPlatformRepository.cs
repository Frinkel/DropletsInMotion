using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface IPlatformRepository
{
    Electrode[][] Board { get; set; }

    double TimeScaleFactor { get; set; }
    double MinimumMovementVolume { get; set; }
    double MaximumMovementVolume { get; set; }
    public double MinSize1x1 { get; set; }
    public double MinSize2x2 { get; set; }
    public double MinSize3x3 { get; set; }
}