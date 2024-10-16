using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface IDeviceRepository
{
    Dictionary<string, Sensor>? Sensors { get; set; }
    Dictionary<string, Actuator>? Actuators { get; set; }
    Dictionary<string, Reservoir>? Reservoirs { get; set; }
    void Initialize();
}