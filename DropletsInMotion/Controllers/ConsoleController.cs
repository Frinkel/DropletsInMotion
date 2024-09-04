using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DropletsInMotion.Controllers
{
    internal class ConsoleController
    {
        public IConfiguration _configuration;

        public bool IsDevelopment { get; private set;  }
        public string? DevelopmentPath { get; private set;  }
        public string? DevelopmentProgram { get; private set; }
        public string? DevelopmentPlatform { get; private set; }

        public ConsoleController(IConfiguration configuration)
        {
            _configuration = configuration;
            IsDevelopment = _configuration.GetValue<bool>("Development:IsDevelopment");
            DevelopmentPath = _configuration["Development:Path"];
            DevelopmentProgram = _configuration["Development:Program"];
            DevelopmentPlatform = _configuration["Development:Platform"];
        }

        public void GetInitialInformation()
        {
            Console.WriteLine("Droplets In Motion - a DMF toolchain");
            GetPathToBoardConfiguration();
            GetPathToProgram();
        }

        public string GetPathToBoardConfiguration()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                while (path == null)
                {
                    Console.Write("Enter the path to your program: ");
                    // TODO: Add validation logic for the path
                    path = Console.ReadLine();
                }
            }
            else
            {
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
                path = projectDirectory + DevelopmentPath + DevelopmentPlatform;
            }

            // This should not happen
            if (path == null)
            {
                throw new ArgumentException("Path to board configuration cannot be null!");
            }

            Console.WriteLine($"Platform configuration path {path}");
            return path;
        }

        public string GetPathToProgram()
        {
            string? path = null;

            if (!IsDevelopment)
            {
                while (path == null)
                {
                    Console.Write("Enter the path to your program: ");
                    // TODO: Add validation logic for the path
                    path = Console.ReadLine();
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

            return path;
        }
    }
}