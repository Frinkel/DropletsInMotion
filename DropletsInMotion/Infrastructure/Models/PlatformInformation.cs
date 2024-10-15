public class MovementInfo
{
    public double MinimumMovementVolume { get; set; }
    public double MaximumMovementVolume { get; set; }
}

public class PlatformInformation
{
    public MovementInfo Movement { get; set; }
}