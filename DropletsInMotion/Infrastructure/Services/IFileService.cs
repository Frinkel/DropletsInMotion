
namespace DropletsInMotion.Infrastructure.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Try to read the file on the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public string ReadFileFromPath(string path);

        /// <summary>
        /// Try to read the file from the project directory
        /// </summary>
        /// <param name="intermediatePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public string ReadFileFromProjectDirectory(string intermediatePath);
        /// <summary>
        /// Get the project directory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetProjectDirectory();

        List<string> GetFilesFromFolder(string path);
    }
}
