using DropletsInMotion.Infrastructure.Models.Commands;

public class StoreManager
{
    public List<(string dropletName, double releaseTime)> StoredDroplets { get; private set; } = new List<(string dropletName, double releaseTime)>();
    private readonly HashSet<string> _dropletNamesInQueue = new HashSet<string>();

    public StoreManager() { }

    public void StoreDroplet(Store storeCommand, double currentTime)
    {
        if (!_dropletNamesInQueue.Contains(storeCommand.DropletName))
        {
            double releaseTime = storeCommand.Time + currentTime;
            string dropletName = storeCommand.DropletName;

            StoredDroplets.Add((dropletName, releaseTime));
            _dropletNamesInQueue.Add(storeCommand.DropletName); // Track it in the HashSet
        }
    }

    public bool ContainsDroplet(string name)
    {
        return _dropletNamesInQueue.Contains(name);
    }

    public void StoreDropletWithNameAndTime(string dropletName, double endTime)
    {
        if (!_dropletNamesInQueue.Contains(dropletName))
        {

            StoredDroplets.Add((dropletName, endTime));
            _dropletNamesInQueue.Add(dropletName); // Track it in the HashSet
        }
    }

    public bool IsStoreComplete(string dropletName, double time)
    {
        var storedDroplet = StoredDroplets.FirstOrDefault(item => item.dropletName == dropletName);

        if (!string.IsNullOrEmpty(storedDroplet.dropletName) && time >= storedDroplet.releaseTime)
        {
            StoredDroplets.Remove(storedDroplet);
            _dropletNamesInQueue.Remove(dropletName);

            return true; // The droplet was found and removed, indicating store completion
        }

        return false; // The droplet either doesn't exist or its store time hasn't completed
    }

    public double PeekClosestTime()
    {
        if (StoredDroplets.Count > 0)
        {
            return StoredDroplets.MinBy(item => item.releaseTime).releaseTime;
        }

        return -1;
    }

    public bool HasStoredDroplets()
    {
        return StoredDroplets.Count > 0;
    }
}