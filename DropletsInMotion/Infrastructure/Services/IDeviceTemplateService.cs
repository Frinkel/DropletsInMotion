namespace DropletsInMotion.Infrastructure.Services;

public interface IDeviceTemplateService
{
    //Dictionary<string, Sensor> Sensors { get; set; }

    void LoadTemplates();
    void PrintAvailableSensors();
}