namespace DropletsInMotion.Infrastructure.Services
{
    public class FileService : IFileService
    {

        public string ReadFileFromPath(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            throw new FileNotFoundException($"The file on path '{path}' was not found.");
        }

        public string ReadFileFromProjectDirectory2(string intermediatePath)
        {
            string path = GetProjectDirectory() + intermediatePath;

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            throw new FileNotFoundException($"The file on path '{path}' was not found.");
        }

        public string ReadFileFromProjectDirectory(string intermediatePath)
        {
            string runtimePath = AppContext.BaseDirectory + intermediatePath;
            if (File.Exists(runtimePath))
            {
                return File.ReadAllText(runtimePath);
            }

            throw new FileNotFoundException($"The file on path '{intermediatePath}' was not found in either development or runtime locations.");
        }

        public string GetProjectDirectory()
        {
            return Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? throw new Exception("Something went wrong when getting the project directory.");
        }

        public List<string> GetFilesFromFolder(string path)
        {
            try
            {
                List<string> files = Directory.GetFiles(path).ToList();
                return files;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Error: The directory '{path}' was not found.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
