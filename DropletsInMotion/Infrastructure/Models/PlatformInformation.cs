public class MovementInfo
{
    public double MinimumMovementVolume { get; set; }
    public double MaximumMovementVolume { get; set; }
    public double MinSize1x1 { get; set; }
    public double MinSize2x2 { get; set; }
    public double MinSize3x3 { get; set; }
}

public class PlatformInformation
{
    public double TimeScaleFactor { get; set; }
    public MovementInfo Movement { get; set; }
}