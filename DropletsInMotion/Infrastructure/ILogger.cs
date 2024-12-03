using Spectre.Console;

namespace DropletsInMotion.Infrastructure;

public interface ILogger
{
    void WriteSuccess(string message);
    void Error(string message);
    void Info(string message);
    void Warning(string message);
    void Debug(string message);
    void WriteEmptyLine(int number);

    void WriteColor(string message, Color foregroundColor = default, Color backgroundColor = default);
    //void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black);
    void WriteColorHex(string message, string foregroundColor = "#FFFFFF", string backgroundColor = "#000000");
    void WriteFiglet(string message);
}