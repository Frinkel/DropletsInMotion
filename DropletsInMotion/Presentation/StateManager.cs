using DropletsInMotion.Application.Execution;
using DropletsInMotion.Communication;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Infrastructure.Services;
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
        //private IDeviceRepository _deviceRepository;

        private readonly ITranslator _translator;
        //private IDeviceTemplateService _deviceTemplateService;

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
                            break;

                        case (ProgramState.CompilingProgram):
                            _currentState = await HandleCompilation();
                            break;

                        case (ProgramState.Completed):
                            _logger.WriteEmptyLine(2);
                            _logger.WriteSuccess("Program compiled successfully!");
                            _logger.WriteEmptyLine(2);
                            _currentState = ProgramState.WaitingForUserInput;
                            break;

                        default:
                            throw new Exception("A non-reachable state was reached");
                    }
                }
                catch (Exception e)
                {
                    //_consoleService.WriteColor("Stopping communication channels...");
                    //_consoleService.WriteEmptyLine(1);
                    //await _communicationEngine.StopCommunication();

                    // TODO: Simplify this for user
                    //_consoleService.WriteColor(e.Message, ConsoleColor.DarkRed);
                    //_logger.Error("An error occurred:");
                    //_logger.Error($"Message: {e.Message}");
                    //_logger.Error($"Source: {e.Source}");
                    //_logger.Error($"TargetSite: {e.TargetSite}");
                    //_logger.Error($"StackTrace: {e.StackTrace}");
                    //_logger.WriteEmptyLine(2);

                    _currentState = ProgramState.WaitingForUserInput;
                    throw;
                }
            }
        }

        private async Task<ProgramState> HandleCompilation()
        {
            //// Parse the contents
            //var inputStream = new AntlrInputStream(_programContent);
            //var lexer = new MicrofluidicsLexer(inputStream);
            //var commonTokenStream = new CommonTokenStream(lexer);
            //var parser = new MicrofluidicsParser(commonTokenStream);
            //var listener = new MicrofluidicsCustomListener();
            //var tree = parser.program();
            //ParseTreeWalker.Default.Walk(listener, tree);

            //// TODO: Remove
            //foreach (var elem in listener.Commands)
            //{
            //    Console.WriteLine(elem.ToString());
            //}

            // TODO: Maybe the compiler should return a status code for the compilation.
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
                    throw new NotImplementedException();
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

            //_deviceTemplateService.LoadTemplates();

            // Switch states
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
                    //_consoleService.WriteColor("User:", ConsoleColor.Blue, ConsoleColor.DarkGreen);
                    string userInput = Console.ReadLine()?.ToLower() ?? "";
                    switch (userInput)
                    {
                        case "start":
                        case "":
                        case null:
                            return ProgramState.CompilingProgram;

                        case "reupload":
                            return ProgramState.GettingInitialInformation;

                        case "disconnect":
                            //await _communicationEngine.StopCommunication()!;
                            return ProgramState.WaitingForClientConnection;

                        case "stop":
                        case "quit":
                        case "q":
                            _logger.WriteColor("Exiting the program...");
                            //await _communicationEngine.StopCommunication()!;
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
            _logger.WriteColor("  reupload         - Re-upload the program");
            _logger.WriteColor("  disconnect       - Disconnect the client and return to waiting for a new connection");
            _logger.WriteColor("  stop / quit / q  - Stop the communication and exit the program");
            _logger.WriteEmptyLine(2);
        }
    }
}
