using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

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

        public ConsoleService(IConfiguration configuration, IUserService userService, IFileService fileService)
        {
            _userService = userService;
            _fileService = fileService;

            var configuration1 = configuration;
            IsDevelopment = configuration1.GetValue<bool>("Development:IsDevelopment");
            DevelopmentPath = configuration1["Development:Path"];
            DevelopmentProgram = configuration1["Development:Program"];
            DevelopmentPlatform = configuration1["Development:Platform"];
            DevelopmentConfiguration = configuration1["Development:Configuration"];
        }

        public void GetInitialInformation()
        {
            _userService.PlatformPath = GetPathToBoardConfiguration();
            _userService.ProgramPath = GetPathToProgram();
            _userService.ConfigurationPath = GetPathToConfiguration();
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
                while (path == null || path?.Trim() == "")
                {
                    Console.Write("Enter the path to your platform configuration: ");
                    // TODO: Add validation logic for the path
                    path = Console.ReadLine();

                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"No file found on path \"{path}\"");
                        path = null;
                    }
                    else if (Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("File is not a JSON file");
                        path = null;
                    }

                }
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


        //public void WriteSuccess(string message)
        //{
        //    WriteColor(message, ConsoleColor.Green);
        //}

        //public void Error(string message)
        //{
        //    WriteColor(message, ConsoleColor.DarkRed);
        //}

        //public void Info(string message)
        //{
        //    WriteColor("Info: " + message, ConsoleColor.DarkCyan);
        //}
        //public void Warning(string message)
        //{
        //    WriteColor("Warning: " + message, ConsoleColor.Yellow);
        //}

        //public void Debug(string message)
        //{
        //    WriteColor("Debug: " + message, ConsoleColor.Green);
        //}

        //public void WriteEmptyLine(int number)
        //{
        //    for (int i = 0; i < number; i++)
        //    {
        //        WriteColor("");
        //    }
        //}

        //public void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        //{
        //    Console.BackgroundColor = backgroundColor;
        //    Console.ForegroundColor = color;
        //    Console.WriteLine(message);

        //    Console.BackgroundColor = ConsoleColor.Black;
        //    Console.ForegroundColor = ConsoleColor.White;
        //}
    }
}