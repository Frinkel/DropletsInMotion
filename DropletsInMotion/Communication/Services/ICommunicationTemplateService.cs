using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication.Services;

public interface ICommunicationTemplateService
{
    //Dictionary<string, Sensor> Sensors { get; set; }

    void LoadTemplates();
    void PrintAvailableSensors();
}