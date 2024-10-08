using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Infrastructure.Repositories;

public interface ISensorRepository
{
    Dictionary<string, Sensor> Sensors { get; set; }
}