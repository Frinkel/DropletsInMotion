using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;

namespace DropletsInMotion.Application.Services
{
    public interface IStoreService
    {
        void StoreDroplet(Store storeCommand, double currentTime);
        bool ContainsDroplet(string name);
        void StoreDropletWithNameAndTime(string dropletName, double endTime);
        bool IsStoreComplete(string dropletName, double time);
        double PeekClosestTime();
        bool HasStoredDroplets();
    }
}
