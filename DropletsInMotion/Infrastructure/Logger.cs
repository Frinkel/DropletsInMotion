using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Infrastructure
{
    public class Logger : ILogger
    {

        private bool _isDebuggingEnabled = false;

        public Logger(IConfiguration configuration)
        {
            _isDebuggingEnabled = configuration.GetValue<bool>("Development:Debugging");
        }

        public void WriteSuccess(string message)
        {
            WriteColor(message, ConsoleColor.Green);
        }

        public void Error(string message)
        {
            WriteColor(message, ConsoleColor.DarkRed);
        }

        public void Info(string message)
        {
            WriteColor("Info: " + message, ConsoleColor.DarkCyan);
        }
        public void Warning(string message)
        {
            WriteColor("Warning: " + message, ConsoleColor.Yellow);
        }

        public void Debug(string message)
        {
            if (!_isDebuggingEnabled) return;
            WriteColor("Debug: " + message, ConsoleColor.Green);
        }

        public void WriteEmptyLine(int number)
        {
            for (int i = 0; i < number; i++)
            {
                WriteColor("");
            }
        }

        public void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
