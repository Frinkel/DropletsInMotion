using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MicrofluidicsParser;

namespace DropletsInMotion.Infrastructure.Services
{
    public class FileService : IFileService
    {
        public FileService()
        {

        }

        public string ReadFileFromPath(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            throw new FileNotFoundException($"The file on path '{path}' was not found.");
        }

        public string ReadFileFromProjectDirectory(string intermediatePath)
        {
            string path = GetProjectDirectory() + intermediatePath;

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            throw new FileNotFoundException($"The file on path '{path}' was not found.");
        }

        public string GetProjectDirectory()
        {
            return Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? throw new Exception("Something went wrong when getting the project directory.");
        }
    }
}
