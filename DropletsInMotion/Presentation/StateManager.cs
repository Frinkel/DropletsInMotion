using DropletsInMotion.Application.Execution;
using DropletsInMotion.Communication;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Translation;
using DropletsInMotion.UI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotion.Presentation
{
    public class StateManager
    {
        CancellationTokenSource _cts = new CancellationTokenSource();


        private readonly IServiceProvider _serviceProvider;
        private readonly IConsoleService _consoleService;
        private readonly ICommunicationEngine _communicationEngine;
        private readonly IUserService _userService;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ITranslator _translator;
        private ProgramState _currentState = ProgramState.GettingInitialInformation;

        private string? _programContent;
        private TaskCompletionSource<bool> _isClientConnected;

        public StateManager(IServiceProvider serviceProvider, IConsoleService consoleService, ICommunicationEngine communicationEngine, 
                            IUserService userService, IFileService fileService, IConfiguration configuration, IDeviceRepository deviceRepository,
                            ITranslator translator, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _communicationEngine = communicationEngine;
            _userService = userService;
            _fileService = fileService;
            _configuration = configuration;
            _logger = logger;
            _isClientConnected = new TaskCompletionSource<bool>();

            _communicationEngine.ClientConnected += OnClientConnected;
            _communicationEngine.ClientDisconnected += OnClientDisconnected;
            _translator = translator;
        }

        public async Task Start()
        {
            while (true)
            {
                try
                {
                    switch (_currentState)
                    {
                        // Get the information that is necessary for the program to run
                        case (ProgramState.GettingInitialInformation):
                            PrintCommands();
                            _consoleService.GetInitialInformation();
                            _translator.Load();
                            _currentState = ProgramState.ReadingInputFiles;
                            break;

                        // Read the files
                        case ProgramState.ReadingInputFiles:
                            _currentState = HandleReadInputFiles();
                            break;

                        case (ProgramState.WaitingForClientConnection):
                            _currentState = await HandleCommunication();
                            break;

                        case (ProgramState.WaitingForUserInput):
                            _currentState = await HandleUserInput(_cts.Token);
                            await Task.Delay(100);
                            break;

                        case (ProgramState.CompilingProgram):
                            _currentState = await HandleCompilation();
                            break;

                        case (ProgramState.Completed):
                            _logger.WriteSuccess("Program compiled successfully!");
                            _logger.WriteEmptyLine(1);
                            _currentState = ProgramState.WaitingForUserInput;
                            break;

                        default:
                            throw new Exception("A non-reachable state was reached");
                    }
                }
                catch (Exception e)
                {
                    _currentState = ProgramState.WaitingForUserInput;
                    throw;
                }
            }
        }

        public void Exit()
        {

        }

        private async Task<ProgramState> HandleCompilation()
        {
            IExecutionEngine executionEngine = _serviceProvider.GetRequiredService<IExecutionEngine>();
            await executionEngine.Execute();

            return ProgramState.Completed;
        }

        private void OnClientConnected(object? sender, EventArgs e)
        {
            _logger.Info("Client connection established!");
            _isClientConnected.TrySetResult(true);
        }

        private async void OnClientDisconnected(object? sender, EventArgs e)
        {
            _logger.Warning("Client disconnected. Resetting the state to wait for a new connection...");
            _currentState = ProgramState.WaitingForClientConnection;
            _isClientConnected = new TaskCompletionSource<bool>();

            _cts.Cancel();
            _cts = new CancellationTokenSource();
        }

        private async Task<ProgramState> HandleCommunication()
        {

            if (_configuration.GetValue<bool>("Development:SimulationCommunication"))
            {
                _userService.Communication = IUserService.CommunicationType.Simulator;
            }

            if (_userService.Communication == IUserService.CommunicationType.NotSet)
            {
                _logger.WriteColor("Enter a communication type:");
                _logger.WriteColor("   0 for Simulation");
                _logger.WriteColor("   1 for Physical Biochip");
                _logger.WriteEmptyLine(1);
                string? userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput) || !userInput.Equals("1"))
                {
                    _userService.Communication = IUserService.CommunicationType.Simulator;
                }
                else
                {
                    _userService.Communication = IUserService.CommunicationType.Physical;
                }
            }

            _logger.WriteEmptyLine(1);
            _logger.Info("Waiting for a client to connect.");
            await _isClientConnected.Task;
            _logger.WriteEmptyLine(1);


            return ProgramState.WaitingForUserInput;
        }

        private ProgramState HandleReadInputFiles()
        {
            _programContent = _fileService.ReadFileFromPath(_userService.ProgramPath);

            if (string.IsNullOrEmpty(_programContent)) throw new InvalidOperationException("The uploaded program cannot be empty!");

            _logger.WriteColor(_programContent);
            _logger.WriteEmptyLine(2);


            return _configuration.GetValue<bool>("Development:SkipCommunication")
                ? ProgramState.WaitingForUserInput
                : ProgramState.WaitingForClientConnection;
        }


        private async Task<ProgramState> HandleUserInput(CancellationToken token)
        {
            _logger.WriteColor("User:");

            while (!token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    string userInput = Console.ReadLine()?.ToLower() ?? "";
                    switch (userInput)
                    {
                        case "start":
                        case "":
                        case null:
                            return ProgramState.CompilingProgram;

                        case "reupload":
                            _userService.ProgramPath = null;
                            _userService.ConfigurationPath = null;
                            _userService.MergeTablePath = null;
                            _userService.ContaminationTablePath = null;
                            return ProgramState.GettingInitialInformation;

                        case "reupload p":
                            _userService.ProgramPath = null;
                            return ProgramState.GettingInitialInformation;

                        case "reupload c":
                            _userService.ConfigurationPath = null;
                            return ProgramState.GettingInitialInformation;

                        case "disconnect":
                            await _communicationEngine.StopCommunication()!;
                            return ProgramState.WaitingForClientConnection;

                        case "stop":
                        case "quit":
                        case "q":
                            _logger.WriteColor("Exiting the program...");
                            await _communicationEngine.StopCommunication()!;
                            Environment.Exit(0);
                            break;

                        case "help":
                        case "?":
                            PrintCommands();
                            return ProgramState.WaitingForUserInput;

                        default:
                            _logger.WriteColor("Invalid command. Please try again.");
                            return ProgramState.WaitingForUserInput;
                    }

                    return ProgramState.WaitingForUserInput;
                }
            }

            return _currentState;
        }

        public void ResetState()
        {
            _currentState = ProgramState.WaitingForUserInput;
        }

        private void PrintCommands()
        {
            _logger.WriteColor("Available commands:");
            _logger.WriteColor("  start / [Enter]  - Start compiling the program");
            _logger.WriteColor("  reupload         - Re-upload the everything");
            _logger.WriteColor("  reupload p       - Re-upload the program");
            _logger.WriteColor("  reupload c       - Re-upload the configuration");
            _logger.WriteColor("  disconnect       - Disconnect the client and return to waiting for a new connection");
            _logger.WriteColor("  stop / quit / q  - Stop the communication and exit the program");
            _logger.WriteEmptyLine(2);
        }
    }
}
