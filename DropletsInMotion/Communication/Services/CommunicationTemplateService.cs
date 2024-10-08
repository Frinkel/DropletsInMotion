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
    private readonly ISensorRepository _sensorRepository;
    //public Dictionary<string, Sensor> Sensors { get; set; }

    public CommunicationTemplateService(IFileService fileService, IUserService userService, ISensorRepository sensorRepository)
    {
        _fileService = fileService;
        _userService = userService;
        _sensorRepository = sensorRepository;
        //Sensors = new Dictionary<string, Sensor>();
    }

    

    public void LoadTemplates()
    {
        LoadSensorTemplates();
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


            _sensorRepository.Sensors.Add(sensor.Name, sensor);
        }
    }

    private Dictionary<string, Sensor> LoadActuatorTemplates()
    {
        throw new NotImplementedException();
    }


    public void PrintAvailableSensors()
    {
        
    }
}
