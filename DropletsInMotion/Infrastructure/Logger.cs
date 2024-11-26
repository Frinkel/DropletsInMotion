namespace DropletsInMotion.Infrastructure
{
    public class Logger : ILogger
    {
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
