using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Infrastructure.Services;

namespace DropletsInMotion.Infrastructure
{
    public class RuntimeExceptionHandler
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IContaminationService _contaminationService;
        public RuntimeExceptionHandler(ILogger logger, IUserService userService, IContaminationService contaminationService)
        {
            _logger = logger;
            _userService = userService;
            _contaminationService = contaminationService;
        }

        public RuntimeExceptionAction Handle(Exception ex)
        {
            _logger.WriteColor("An error occured while compiling the protocol!", ConsoleColor.Yellow, ConsoleColor.DarkRed);

            switch (ex)
            {
                case DropletNotFoundException dropletNotFoundException:
                    _logger.WriteColor($"Error Message: \"{dropletNotFoundException.Message}\"", ConsoleColor.Red);
                    return RuntimeExceptionAction.Reset;

                case CommandException typeCheckerException:
                    _logger.WriteColor($"Exception occured in file: '{_userService.ProgramPath}'", ConsoleColor.Red);

                    _logger.WriteColor($"On line {typeCheckerException.Command.Line} in '{typeCheckerException.Command}':" +
                                       $"\n\tError Message: \"{typeCheckerException.Message}\"", ConsoleColor.Red);
                    _logger.WriteEmptyLine(1);
                    _logger.WriteColor("Tip: Fix the issue and re-upload the program with the command 'reupload'", ConsoleColor.Yellow);
                    return RuntimeExceptionAction.Reset;

                case ContaminationException contaminationException:
                    _logger.WriteColor($"Exception connected to the resulting contamination.", ConsoleColor.Red);
                    _logger.WriteColor($"Error Message: \"{contaminationException.Message}\"", ConsoleColor.Red);
                    _logger.WriteColor($"Contamination map:", ConsoleColor.Red);
                    _contaminationService.PrintContaminationMap(contaminationException.ContaminationMap);
                    return RuntimeExceptionAction.Reset;

                case TemplateException templateException:
                    _logger.WriteColor($"Exception occured related to \"{templateException.Template.Name}\".");
                    _logger.WriteColor($"Error Message: \"{templateException.Message}\"", ConsoleColor.Red);

                    return RuntimeExceptionAction.Reset;

                case PositionUnreachableException positionUnreachableException:
                    _logger.WriteColor($"E");
                    return RuntimeExceptionAction.Reset;

                case DirectoryNotFoundException directoryNotFoundException:
                    _logger.WriteColor($"Directory not found: {directoryNotFoundException.Message}");
                    return RuntimeExceptionAction.Reset;

                default:
                    _logger.Warning($"Unhandled exception: {ex.Message}");
                    return RuntimeExceptionAction.Exit;
            }
        }

        public enum RuntimeExceptionAction
        {
            Reset,
            Exit
        }
    }
}