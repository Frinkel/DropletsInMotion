using System.Text.Json;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;

namespace DropletsInMotion.Infrastructure.Services;
public class DeviceTemplateService : IDeviceTemplateService
{
    private readonly IFileService _fileService;
    private readonly IUserService _userService;
    private readonly IDeviceRepository _deviceRepository;
    //public Dictionary<string, Sensor> Sensors { get; set; }

    public DeviceTemplateService(IFileService fileService, IUserService userService, IDeviceRepository deviceRepository)
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
        LoadReservoirTemplates();
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

    private void LoadReservoirTemplates()
    {
        string reservoirFolderPath = _userService.ConfigurationPath + "/Reservoirs";

        List<string> reservoirPaths = _fileService.GetFilesFromFolder(reservoirFolderPath);
        foreach (var reservoirPath in reservoirPaths)
        {
            string content = _fileService.ReadFileFromPath(reservoirPath);
            Reservoir reservoir = JsonSerializer.Deserialize<Reservoir>(content) ?? throw new InvalidOperationException("A sensor configuration did not correspond to the expected format!");

            Console.WriteLine($"Added reservoir:\n{reservoir}");

            _deviceRepository.Reservoirs.Add(reservoir.Name, reservoir);
        }
    }


    public void PrintAvailableSensors()
    {
        
    }
}
