using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface IDeviceRepository
{
    Dictionary<string, Sensor> Sensors { get; set; }
    Dictionary<string, Actuator> Actuators { get; set; }
}