using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Infrastructure.Repositories;
public class DeviceRepository : IDeviceRepository
{
    public Dictionary<string, Sensor> Sensors { get; set; }
    public Dictionary<string, Actuator> Actuators { get; set; }

    public DeviceRepository()
    {
        Sensors = new Dictionary<string, Sensor>();
        Actuators = new Dictionary<string, Actuator>();
    }
}
