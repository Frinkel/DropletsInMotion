namespace DropletsInMotion.Application.Services
{
    public interface ITimeService
    {
        double? CalculateBoundTime(double currentTime, double? boundTime);
    }

}
