using System.Dynamic;
using System.Text.Json;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Communication.Services;
public class CommunicationTemplateService : ICommunicationTemplateService
{
    private readonly IFileService _fileService;
    private readonly IUserService _userService;
    private readonly IDeviceRepository _deviceRepository;
    //public Dictionary<string, Sensor> Sensors { get; set; }

    public CommunicationTemplateService(IFileService fileService, IUserService userService, IDeviceRepository deviceRepository)
    {
        _fileService = fileService;
        _userService = userService;
        _deviceRepository = deviceRepository;
        //Sensors = new Dictionary<string, Sensor>();
    }

    

    public void LoadTemplates()
    {
        LoadSensorTemplates();
        LoadActuatorTemplates();
    }

    private void LoadSensorTemplates()
    {
        string sensorFolderPath = _userService.ConfigurationPath + "/Sensors";

        List<string> sensorPaths = _fileService.GetFilesFromFolder(sensorFolderPath);
        foreach (var sensorPath in sensorPaths)
        {
            string content = _fileService.ReadFileFromPath(sensorPath);
            Sensor sensor = JsonSerializer.Deserialize<Sensor>(content) ?? throw new InvalidOperationException("A sensor configuration did not correspond to the expected format!");

            Console.WriteLine($"Added sensor {sensor.Name}");

            foreach (var kvp in sensor.ArgumentHandlers)
            {
                Console.WriteLine($"Argument {kvp.Key}:\n{kvp.Value.Request}");
            }


            _deviceRepository.Sensors.Add(sensor.Name, sensor);
        }
    }

    private void LoadActuatorTemplates()
    {
        string actuatorFolderPath = _userService.ConfigurationPath + "/Actuators";

        List<string> actuatorPaths = _fileService.GetFilesFromFolder(actuatorFolderPath);
        foreach (var actuatorPath in actuatorPaths)
        {
            string content = _fileService.ReadFileFromPath(actuatorPath);
            Actuator actuator = JsonSerializer.Deserialize<Actuator>(content) ?? throw new InvalidOperationException("A sensor configuration did not correspond to the expected format!");

            Console.WriteLine($"Added actuator {actuator.Name}");

            foreach (var kvp in actuator.Arguments)
            {
                Console.WriteLine($"Argument {kvp.Key}:\n{kvp.Value}");
            }


            _deviceRepository.Actuators.Add(actuator.Name, actuator);
        }
    }


    public void PrintAvailableSensors()
    {
        
    }
}
