using System.Drawing;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Exceptions;
using Microsoft.CSharp.RuntimeBinder;

namespace DropletsInMotion.Presentation
{
    public class ConsoleService : IConsoleService
    {
        private IUserService _userService;
        private IFileService _fileService;
        private ILogger _logger;

        public bool IsDevelopment { get; private set; }
        public string? DevelopmentPath { get; private set; }
        public string? DevelopmentProgram { get; private set; }
        public string? DevelopmentPlatform { get; private set; }
        public string? DevelopmentConfiguration { get; private set; }
        public string? DevelopmentContaminationTable { get; private set; }
        public string? DevelopmentMergeTable { get; private set; }

        public ConsoleService(IConfiguration configuration, IUserService userService, IFileService fileService, ILogger logger)
        {
            _userService = userService;
            _fileService = fileService;
            _logger = logger;

            IsDevelopment = configuration.GetValue<bool>("Development:IsDevelopment");
            DevelopmentPath = configuration["Development:Path"];
            DevelopmentProgram = configuration["Development:Program"];
            DevelopmentPlatform = configuration["Development:Platform"];
            DevelopmentConfiguration = configuration["Development:Configuration"];
            DevelopmentContaminationTable = configuration["Development:ContaminationTable"];
            DevelopmentMergeTable = configuration["Development:MergeTable"];
        }

        public void GetInitialInformation()
        {
            _userService.ConfigurationPath ??= GetPathToConfiguration();

            _userService.ProgramPath ??= GetPathToProgram();
            _userService.PlatformPath ??= GetPathToPlatform();
            _userService.ContaminationTablePath ??= GetPathToContaminationTable();
            _userService.MergeTablePath ??= GetPathToMergeTable();
        }

        private string ReadFileFromUserPath(string message, string format)
        {
            string? path = null;

            while (path == null || path?.Trim() == "")
            {
                Console.Write(message + "\n");
                path = Console.ReadLine();

                if (!File.Exists(path))
                {
                    Console.WriteLine($"No file found on path \"{path}\"");
                    path = null;
                }
                else if (!Path.GetExtension(path).Equals(format, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"File is not on the correct format \"{format}\"");
                    path = null;
                }
            }

            if (path == null)
            {
                throw new RuntimeException("Path cannot be null!");
            }

            return path;
        }

        private string ReadFolderFromUserPath(string message)
        {
            string? path = null;

            // Keep asking for input until a valid folder path is provided
            while (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                Console.Write(message + "\n");
                path = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                {
                    Console.WriteLine($"No folder found at path \"{path}\". Please try again.");
                    path = null;
                }
            }

            return path ?? throw new InvalidOperationException("Path cannot be null!");
        }

        private string? ReadPathFromUserOptional(string message, string format)
        {
            string? path = null;

            while (path == null)
            {
                Console.Write("(Optional) " + message + "\n");
                path = Console.ReadLine();

                if (File.Exists(path) && !Path.GetExtension(path).Equals(format, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"File is not on the correct format \"{format}\"");
                    path = null;
                }
            }

            if (!File.Exists(path) || path?.Trim() == "")
            {
                path = null;
            }

            return path;
        }

        private string? GetPathToContaminationTable()
        {
            string? path = null;

            if (IsDevelopment)
            {
                path = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentContaminationTable;
                if (!File.Exists(_userService.ContaminationTablePath) || _userService.ContaminationTablePath?.Trim() == "")
                {
                    return null;
                }

                Console.WriteLine($"ContaminationTablePath configuration path {_userService.ContaminationTablePath}\n");

                return path;
            }

            _userService.ContaminationTablePath = ReadPathFromUserOptional("Enter the path to the contamination relation table:", ".csv");
            
            if (!File.Exists(_userService.ContaminationTablePath) || _userService.ContaminationTablePath?.Trim() == "")
            {
                return null;
            }

            return path;
        }


        private string? GetPathToMergeTable()
        {
            string? path = null;

            if (IsDevelopment)
            {
                path = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentMergeTable;
                if (!File.Exists(_userService.MergeTablePath) || _userService.MergeTablePath?.Trim() == "")
                {
                    return null;
                }

                Console.WriteLine($"ContaminationTablePath configuration path {_userService.MergeTablePath}\n");

                return path;
            }

            _userService.MergeTablePath = ReadPathFromUserOptional("Enter the path to the contamination relation table:", ".csv");

            if (!File.Exists(_userService.MergeTablePath) || _userService.MergeTablePath?.Trim() == "")
            {
                return null;
            }

            return path;
        }


        private string? GetFilePathInFolder(string fileName, string fileExtension, string folderPath)
        {
            string searchPattern = $"{fileName}.{fileExtension}";
            string[] matchingFiles = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);

            return matchingFiles.Length > 0 ? matchingFiles.First() : null;
        }


        private void ValidateConfiguration(string folderPath)
        {
            // Validate folders
            string actuatorFolder = Directory.GetDirectories(folderPath, "actuators", SearchOption.TopDirectoryOnly)
                .SingleOrDefault() ?? throw new DirectoryNotFoundException("Actuators folder is missing from the Configuration.");

            string sensorsFolder = Directory.GetDirectories(folderPath, "sensors", SearchOption.TopDirectoryOnly)
                .SingleOrDefault() ?? throw new DirectoryNotFoundException("Sensors folder is missing from the Configuration.");

            string reservoirFolder = Directory.GetDirectories(folderPath, "reservoirs", SearchOption.TopDirectoryOnly)
                .SingleOrDefault() ?? throw new DirectoryNotFoundException("Reservoirs folder is missing from the Configuration.");
            
            string templatesFolder = Directory.GetDirectories(folderPath, "templates", SearchOption.TopDirectoryOnly)
                .SingleOrDefault() ?? throw new DirectoryNotFoundException("Templates folder is missing from the Configuration.");

            //var first = macthingFolders.First(folder => folder.Contains("actuators"));


            Console.WriteLine(actuatorFolder);


            // Validate important files
            string[] platformInformation = Directory.GetFiles(folderPath, "PlatformInformation.json", SearchOption.TopDirectoryOnly);
            if (platformInformation.Length == 0) throw new FileNotFoundException("The PlatformInformation.json is missing from the Configuration.");

        }


        private string GetPathToConfiguration()
        {
            string? path = null;

            if (IsDevelopment)
            {
                path = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentConfiguration;
                return path;
            }

            path = ReadFolderFromUserPath("Enter path to the configuration folder:");

            // This should in theory not happen
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("The configuration folder does not exist!");


            ValidateConfiguration(path);
            //bool isConfigurationValid = IsConfigurationValid(path);
            //if (!isConfigurationValid) throw new RuntimeException("The configuration structure is invalid.");



            List<string?> foundFiles = new List<string?>();


            // TODO: Extract to another function 
            var program = GetFilePathInFolder("program", "txt", path);
            var platform = GetFilePathInFolder("platform", "json", path);
            var contaminationTable = GetFilePathInFolder("contaminationTable", "csv", path);
            var mergeTable = GetFilePathInFolder("mergeTable", "csv", path);

            foundFiles.Add(program);
            foundFiles.Add(platform);
            foundFiles.Add(contaminationTable);
            foundFiles.Add(mergeTable);

            _logger.WriteEmptyLine(1);
            _logger.WriteColor("The following files were found in the configurations folder:");
            foreach (var file in foundFiles)
            {
                if (file == null) continue;
                _logger.WriteColor($"\t{Path.GetFileName(file)}", ConsoleColor.Blue);
            }

            _logger.WriteColor("Do you want to use these files for the compilation?");
            _logger.WriteColor("(Yes)/No: ");
            var response = Console.ReadLine();
            response = response?.Trim();

            if (response != null && (response == "" || response.ToLower() == "yes" || response.ToLower() == "y"))
            {
                // Load files
                _logger.Debug("Load Files");

                _userService.ProgramPath = program;
                _userService.PlatformPath = platform;
                _userService.ContaminationTablePath = contaminationTable;
                _userService.MergeTablePath = mergeTable;

            }


            if (path == null)
            {
                throw new ArgumentException("Path to board configuration cannot be null!");
            }

            Console.WriteLine($"Platform configuration path {path}\n");
            return path;
        }

        public string GetPathToPlatform()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                path = ReadFileFromUserPath("Enter the path to the contamination relation table:", ".csv");
            }
            else
            {
                path = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentPlatform;
            }

            if (path == null)
            {
                throw new ArgumentException("Path to board configuration cannot be null!");
            }

            Console.WriteLine($"Platform configuration path {path}\n");
            return path;
        }

        public string GetPathToProgram()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                while (path == null || path?.Trim() == "")
                {
                    Console.Write("Enter the path to your program: ");
                    // TODO: Add validation logic for the path
                    path = Console.ReadLine();

                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"No file found on path \"{path}\"");
                        path = null;
                    }
                }
            }
            else
            {
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
                path = projectDirectory + DevelopmentPath + DevelopmentProgram;
            }

            if (path == null)
            {
                throw new ArgumentException("Path to program cannot be null!");
            }

            Console.WriteLine($"Program path {path}\n");
            return path;
        }
    }
}