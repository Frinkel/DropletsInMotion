namespace DropletsInMotion.Presentation
{
    public interface IConsoleService
    {
        // Properties
        bool IsDevelopment { get; }
        string? DevelopmentPath { get; }
        string? DevelopmentProgram { get; }
        string? DevelopmentPlatform { get; }

        // Methods
        void GetInitialInformation();
        string GetPathToBoardConfiguration();
        string GetPathToProgram();
        //void WriteSuccess(string message);
        //void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black);
        //void WriteEmptyLine(int number);
        //void Error(string message);
        //void Info(string message);
        //void Warning(string message);
        //void Debug(string message);
    }
}