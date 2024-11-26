namespace DropletsInMotion.Infrastructure;

public interface ILogger
{
    void WriteSuccess(string message);
    void Error(string message);
    void Info(string message);
    void Warning(string message);
    void Debug(string message);
    void WriteEmptyLine(int number);
    void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black);
}