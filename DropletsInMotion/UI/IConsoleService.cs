namespace DropletsInMotion.UI
{
    public interface IConsoleService
    {
        // Properties
        string ProgramPath { get; set; }
        string PlatformPath { get; set; }
        bool IsDevelopment { get; }
        string? DevelopmentPath { get; }
        string? DevelopmentProgram { get; }
        string? DevelopmentPlatform { get; }

        // Methods
        void GetInitialInformation();
        string GetPathToBoardConfiguration();
        string GetPathToProgram();
        void WriteSuccess(string message);
        void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black);
    }
}