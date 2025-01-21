using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;
public class DeviceRepository : IDeviceRepository
{
    public Dictionary<string, Sensor>? Sensors { get; set; }
    public Dictionary<string, Actuator>? Actuators { get; set; }
    public Dictionary<string, Reservoir>? Reservoirs { get; set; }

    public DeviceRepository()
    {
        Initialize();
    }

    public void Initialize()
    {
        Sensors = new Dictionary<string, Sensor>();
        Actuators = new Dictionary<string, Actuator>();
        Reservoirs = new Dictionary<string, Reservoir>();
    }
}
