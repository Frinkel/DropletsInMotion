using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using DropletsInMotion.Infrastructure.Exceptions;

namespace DropletsInMotion.Presentation
{
    public class ConsoleService : IConsoleService
    {
        private IUserService _userService;
        private IFileService _fileService;

        public bool IsDevelopment { get; private set; }
        public string? DevelopmentPath { get; private set; }
        public string? DevelopmentProgram { get; private set; }
        public string? DevelopmentPlatform { get; private set; }
        public string? DevelopmentConfiguration { get; private set; }
        public string? DevelopmentContaminationTable { get; private set; }
        public string? DevelopmentMergeTable { get; private set; }

        public ConsoleService(IConfiguration configuration, IUserService userService, IFileService fileService)
        {
            _userService = userService;
            _fileService = fileService;

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
            _userService.PlatformPath = GetPathToBoardConfiguration();
            _userService.ProgramPath = GetPathToProgram();
            _userService.ConfigurationPath = GetPathToConfiguration();
            GetPathToContaminationConfiguration();
        }

        private string ReadPathFromUser(string message, string format)
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
                else if (Path.GetExtension(path).Equals(format, StringComparison.OrdinalIgnoreCase))
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

        private string? ReadPathFromUserOptional(string message, string format)
        {
            string? path = null;

            while (path == null)
            {
                Console.Write("(Optional) " + message + "\n");
                path = Console.ReadLine();

                if (File.Exists(path) && Path.GetExtension(path).Equals(format, StringComparison.OrdinalIgnoreCase))
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


        private void GetPathToContaminationConfiguration()
        {

            if (IsDevelopment)
            {
                _userService.ContaminationTablePath = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentContaminationTable;
                _userService.MergeTablePath = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentMergeTable;

                if (!File.Exists(_userService.ContaminationTablePath) || _userService.ContaminationTablePath?.Trim() == "")
                {
                    _userService.ContaminationTablePath = null;
                }

                if (!File.Exists(_userService.MergeTablePath) || _userService.MergeTablePath?.Trim() == "")
                {
                    _userService.MergeTablePath = null;
                }

                Console.WriteLine($"ContaminationTablePath configuration path {_userService.ContaminationTablePath}\n");
                Console.WriteLine($"MergeTablePath configuration path {_userService.MergeTablePath}\n");

                return;
            }

            _userService.ContaminationTablePath = ReadPathFromUserOptional("Enter the path to the contamination relation table:", ".csv");
            _userService.MergeTablePath = ReadPathFromUserOptional("Enter the path to the merge relation table:", ".csv");
        }

        private string GetPathToConfiguration()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                throw new NotImplementedException("We cant do this yet!");
                //while (path == null || path?.Trim() == "")
                //{
                //    Console.Write("Enter the path to your configuration: ");
                //    // TODO: Add validation logic for the path
                //    path = Console.ReadLine();

                //    if (!File.Exists(path))
                //    {
                //        Console.WriteLine($"No file found on path \"{path}\"");
                //        path = null;
                //    }
                //    else if (Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
                //    {
                //        Console.WriteLine("File is not a JSON file");
                //        path = null;
                //    }

                //}
            }
            else
            {
                path = _fileService.GetProjectDirectory() + DevelopmentPath + DevelopmentConfiguration;
            }

            if (path == null)
            {
                throw new ArgumentException("Path to board configuration cannot be null!");
            }

            Console.WriteLine($"Platform configuration path {path}\n");
            return path;
        }

        public string GetPathToBoardConfiguration()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                path = ReadPathFromUser("Enter the path to the contamination relation table:", ".csv");
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