using Spectre.Console;


namespace DropletsInMotion.Infrastructure
{
    public class Logger : ILogger
    {
        public void WriteSuccess(string message)
        {
            WriteColor(message, Color.Green);
        }

        public void Error(string message)
        {
            WriteColor(message, Color.DarkRed);
        }

        public void Info(string message)
        {
            WriteColor("Info: " + message, Color.DarkCyan);
        }
        public void Warning(string message)
        {
            WriteColor("Warning: " + message, Color.Yellow);
        }

        public void Debug(string message)
        {
            WriteColor("Debug: " + message, Color.Green);
        }

        public void WriteEmptyLine(int number)
        {
            for (int i = 0; i < number; i++)
            {
                WriteColor("");
            }
        }

        //public void WriteColor(string message, ConsoleColor color = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        //{
        //    Console.BackgroundColor = backgroundColor;
        //    Console.ForegroundColor = color;
        //    Console.Write(message);
        //    Console.ResetColor();
        //    Console.WriteLine();
        //}


        public void WriteColor(string message, Color foregroundColor = default, Color backgroundColor = default)
        {
            foregroundColor = foregroundColor == default ? Color.White : foregroundColor;
            backgroundColor = backgroundColor == default ? Color.Black : backgroundColor;

            AnsiConsole.Markup($"[{foregroundColor} on {backgroundColor}]{message}[/]");
            AnsiConsole.WriteLine();
        }

        public void WriteColorHex(string message, string foregroundColor = "#FFFFFF", string backgroundColor = "#000000")
        {
            // Use hex colors in markup
            AnsiConsole.Markup($"[{foregroundColor} on {backgroundColor}]{message}[/]");
            AnsiConsole.WriteLine();
        }

        public void WriteFiglet(string message)
        {
            AnsiConsole.Write(
                new FigletText(message)
                    .LeftJustified()
                    .Color(Color.Red));
        }
    }
}
