using System.Text.Json.Serialization;
using System.Text.Json;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Infrastructure.Repositories;
using System.IO;

namespace DropletsInMotion.Presentation.Services
{
    public class PlatformService : IPlatformService
    {
        public Electrode[][] Board { get; set; }
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly IPlatformRepository _platformRepository;

        public PlatformService(IFileService fileService, IUserService userService, IPlatformRepository platformRepository)
        {
            _fileService = fileService;
            _userService = userService;
            _platformRepository = platformRepository;
        }
        public void LoadBoardFromJson(string jsonFilePath)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
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

        public void LoadPlatformInformation()
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
