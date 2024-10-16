﻿using System.Text.Json.Serialization;
using System.Text.Json;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Infrastructure.Repositories;
using System.IO;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Presentation.Services
{
    public class PlatformService : IPlatformService
    {
        public Electrode[][]? Board { get; set; }
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly IPlatformRepository _platformRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ITemplateRepository _templateRepository;

        public PlatformService(IFileService fileService, IUserService userService, IPlatformRepository platformRepository, IDeviceRepository deviceRepository, ITemplateRepository templateRepository)
        {
            _fileService = fileService;
            _userService = userService;
            _platformRepository = platformRepository;
            _deviceRepository = deviceRepository;
            _templateRepository = templateRepository;
        }


        public void Load()
        {
            _deviceRepository.Initialize();
            _templateRepository.Initialize();

            LoadBoardFromJson();
            LoadPlatformInformation();
            LoadSensorTemplates();
            LoadActuatorTemplates();
            LoadReservoirTemplates();
            LoadSplitTemplates();
            LoadRavelTemplates();
            LoadUnravelTemplates();

        }

        private void LoadSplitTemplates()
        {
            string splitFolderPath = _userService.ConfigurationPath + "/Templates/Split";

            List<string> splitPaths = _fileService.GetFilesFromFolder(splitFolderPath);
            foreach (var splitPath in splitPaths)
            {
                string[] contentArr = _fileService.ReadFileFromPath(splitPath).Split("?");

                if (contentArr.Length < 2)
                {
                    throw new InvalidOperationException($"Split template \"{splitPath}\" is missing information!");
                }

                string content = contentArr[0];
                string template = contentArr[1];
                SplitTemplate splitTemplate = JsonSerializer.Deserialize<SplitTemplate>(content) ?? throw new InvalidOperationException("A split template configuration did not correspond to the expected format!");

                Console.WriteLine($"Added split template: {splitTemplate.Name}");

                _templateRepository.AddSplit(splitTemplate, template);
            }
        }

        private void LoadRavelTemplates()
        {
            string ravelFolderPath = _userService.ConfigurationPath + "/Templates/Ravel";

            List<string> ravelPaths = _fileService.GetFilesFromFolder(ravelFolderPath);
            foreach (var ravelPath in ravelPaths)
            {
                string[] contentArr = _fileService.ReadFileFromPath(ravelPath).Split("?");

                if (contentArr.Length < 2)
                {
                    throw new InvalidOperationException($"Ravel template \"{ravelPaths}\" is missing information!");
                }

                string content = contentArr[0];
                string template = contentArr[1];
                RavelTemplate ravelTemplate = JsonSerializer.Deserialize<RavelTemplate>(content) ?? throw new InvalidOperationException("A ravel template configuration did not correspond to the expected format!");

                Console.WriteLine($"Added ravel template: {ravelTemplate.Name}");

                _templateRepository.AddRavel(ravelTemplate, template);
            }
        }

        private void LoadUnravelTemplates()
        {
            string unravelFolderPath = _userService.ConfigurationPath + "/Templates/Unravel";

            List<string> unravelPaths = _fileService.GetFilesFromFolder(unravelFolderPath);
            foreach (var unravelPath in unravelPaths)
            {
                string[] contentArr = _fileService.ReadFileFromPath(unravelPath).Split("?");

                if (contentArr.Length < 2)
                {
                    throw new InvalidOperationException($"Unravel template \"{unravelPaths}\" is missing information!");
                }

                string content = contentArr[0];
                string template = contentArr[1];
                UnravelTemplate unravelTemplate = JsonSerializer.Deserialize<UnravelTemplate>(content) ?? throw new InvalidOperationException("A unravel template configuration did not correspond to the expected format!");

                Console.WriteLine($"Added unravel template: {unravelTemplate.Name}");

                _templateRepository.AddUnravel(unravelTemplate, template);
            }
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

        public void LoadBoardFromJson()
        {
            string jsonContent = File.ReadAllText(_userService.PlatformPath);
            RootObject rootObject = JsonSerializer.Deserialize<RootObject>(jsonContent);

            // Filter electrodes with names starting with "arrel"
            var filteredElectrodes = rootObject.Electrodes
                .Where(e => e.Name.StartsWith("arrel", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Determine the smallest and largest X and Y coordinates
            int minX = filteredElectrodes.Min(e => e.PositionX);
            int maxX = filteredElectrodes.Max(e => e.PositionX);
            int minY = filteredElectrodes.Min(e => e.PositionY);
            int maxY = filteredElectrodes.Max(e => e.PositionY);

            // Determine board dimensions based on the electrode size
            int gridSizeX = (maxX - minX) / filteredElectrodes[0].SizeX + 1;
            int gridSizeY = (maxY - minY) / filteredElectrodes[0].SizeY + 1;

            // Initialize the board
            Board = new Electrode[gridSizeX][];

            for (int i = 0; i < gridSizeX; i++)
            {
                Board[i] = new Electrode[gridSizeY];
            }

            // Place electrodes on the board
            foreach (var electrodeJson in filteredElectrodes)
            {
                int x = (electrodeJson.PositionX - minX) / electrodeJson.SizeX;
                int y = (electrodeJson.PositionY - minY) / electrodeJson.SizeY;

                Board[x][y] = new Electrode(electrodeJson.Id, x, y);
            }

            _platformRepository.Board = Board;
        }

        private void LoadPlatformInformation()
        {
            string platformInformationPath = _userService.ConfigurationPath + "/PlatformInformation.json";

            string fileContent = _fileService.ReadFileFromPath(platformInformationPath);

            var platformInfo = JsonSerializer.Deserialize<PlatformInformation>(fileContent);

            _platformRepository.MinimumMovementVolume = platformInfo.Movement.MinimumMovementVolume;
            _platformRepository.MaximumMovementVolume = platformInfo.Movement.MaximumMovementVolume;
        }

        public void PrintBoard()
        {
            // Determine the maximum number of digits for proper alignment
            int maxDigits = Board
                .SelectMany(row => row)
                .Where(electrode => electrode != null)
                .Max(electrode => electrode.Id)
                .ToString().Length;

            int rowCount = Board.Length;
            int colCount = Board[0].Length;

            for (int j = 0; j < colCount; j++)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    if (Board[i][j] != null)
                    {
                        // Print each ElectrodeId with a fixed width
                        Console.Write(Board[i][j].Id.ToString().PadLeft(maxDigits) + " ");
                    }
                    else
                    {
                        // Print an empty space with the same width
                        Console.Write(new string(' ', maxDigits) + " ");
                    }
                }
                Console.WriteLine();
            }
        }

        public class Platform
        {
            public string PlatformName { get; set; }
            public string PlatformType { get; set; }
            public int PlatformID { get; set; }
            public int SizeX { get; set; }
            public int SizeY { get; set; }
        }

        public class ElectrodeJson
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("ID")]
            public int Id { get; set; }
            [JsonPropertyName("electrodeID")]
            public int ElectrodeID { get; set; }
            [JsonPropertyName("driverID")]
            public int DriverID { get; set; }
            [JsonPropertyName("positionX")]
            public int PositionX { get; set; }
            [JsonPropertyName("positionY")]
            public int PositionY { get; set; }
            [JsonPropertyName("sizeX")]
            public int SizeX { get; set; }
            [JsonPropertyName("sizeY")]
            public int SizeY { get; set; }
            [JsonPropertyName("status")]
            public int Status { get; set; }
        }

        public class RootObject
        {
            [JsonPropertyName("information")]
            public Platform Information { get; set; }
            [JsonPropertyName("electrodes")]
            public List<ElectrodeJson> Electrodes { get; set; }
        }
    }
}
