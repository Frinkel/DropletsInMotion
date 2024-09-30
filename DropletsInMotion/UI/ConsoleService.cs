using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.UI
{
    public class ConsoleService : IConsoleService
    {
        public IConfiguration _configuration;

        public string ProgramPath { get; set; }
        public string PlatformPath { get; set; }

        public bool IsDevelopment { get; private set; }
        public string? DevelopmentPath { get; private set; }
        public string? DevelopmentProgram { get; private set; }
        public string? DevelopmentPlatform { get; private set; }

        public ConsoleService(IConfiguration configuration)
        {
            _configuration = configuration;
            IsDevelopment = _configuration.GetValue<bool>("Development:IsDevelopment");
            DevelopmentPath = _configuration["Development:Path"];
            DevelopmentProgram = _configuration["Development:Program"];
            DevelopmentPlatform = _configuration["Development:Platform"];
        }

        public void GetInitialInformation()
        {
            PlatformPath = GetPathToBoardConfiguration();
            ProgramPath = GetPathToProgram();
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
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
                path = projectDirectory + DevelopmentPath + DevelopmentPlatform;
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


        public void WriteSuccess(string message)
        {
            WriteColor(message, ConsoleColor.Green);
        }

        public void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}