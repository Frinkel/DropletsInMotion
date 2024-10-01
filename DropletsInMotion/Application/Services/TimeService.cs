namespace DropletsInMotion.Application.Services
{

    public class TimeService : ITimeService
    {
        private readonly IStoreService _storeService;

        public TimeService(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public double? CalculateBoundTime(double currentTime, double? boundTime)
        {
            if (_storeService.HasStoredDroplets())
            {
                double nextStoreTime = _storeService.PeekClosestTime();
                return boundTime > currentTime ? boundTime > nextStoreTime ? nextStoreTime : boundTime : nextStoreTime;
            }

            return boundTime > currentTime ? boundTime : null;
        }
    }
}
