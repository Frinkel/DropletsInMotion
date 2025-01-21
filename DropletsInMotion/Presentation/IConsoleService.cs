namespace DropletsInMotion.Presentation
{
    public interface IConsoleService
    {
        bool IsDevelopment { get; }
        string? DevelopmentPath { get; }
        string? DevelopmentProgram { get; }
        string? DevelopmentPlatform { get; }

        void GetInitialInformation();
        string GetPathToPlatform();
        string GetPathToProgram();
    }
}