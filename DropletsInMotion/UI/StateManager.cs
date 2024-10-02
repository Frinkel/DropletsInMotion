using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Communication;
using DropletsInMotion.Presentation.Language;
using DropletsInMotion.UI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using static MicrofluidicsParser;
using System.Runtime.InteropServices;

namespace DropletsInMotion.UI
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

        private ProgramState _currentState = ProgramState.GettingInitialInformation;

        private string? _programContent;
        private TaskCompletionSource<bool> _isClientConnected;

        public StateManager(IServiceProvider serviceProvider, IConsoleService consoleService, ICommunicationEngine communicationEngine, IUserService userService, IFileService fileService, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _communicationEngine = communicationEngine;
            _userService = userService;
            _fileService = fileService;
            _configuration = configuration;
            _isClientConnected = new TaskCompletionSource<bool>();

            _communicationEngine.ClientConnected += OnClientConnected;
            _communicationEngine.ClientDisconnected += OnClientDisconnected;
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
                            _consoleService.WriteEmptyLine(2);
                            _consoleService.WriteSuccess("Program compiled successfully!");
                            _consoleService.WriteEmptyLine(2);
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
                    _consoleService.Error("An error occurred:");
                    _consoleService.Error($"Message: {e.Message}");
                    _consoleService.Error($"Source: {e.Source}");
                    _consoleService.Error($"TargetSite: {e.TargetSite}");
                    _consoleService.Error($"StackTrace: {e.StackTrace}");

                    _consoleService.WriteEmptyLine(2);
                    _currentState = ProgramState.WaitingForUserInput;
                    throw;
                }
            }
        }

        private async Task<ProgramState> HandleCompilation()
        {
            // Parse the contents
            var inputStream = new AntlrInputStream(_programContent);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);
            var listener = new MicrofluidicsCustomListener();
            var tree = parser.program();
            ParseTreeWalker.Default.Walk(listener, tree);

            // TODO: Remove
            foreach (var elem in listener.Commands)
            {
                Console.WriteLine(elem.ToString());
            }

            // TODO: Maybe the compiler should return a status code for the compilation.
            IExecutionEngine executionEngine = _serviceProvider.GetRequiredService<IExecutionEngine>();
            executionEngine.Agents.Clear(); // TODO: We need to clear this because of persistance
            await executionEngine.Execute();


            return ProgramState.Completed;
        }

        private void OnClientConnected(object? sender, EventArgs e)
        {
            _consoleService.Info("Client connection established!");
            _isClientConnected.TrySetResult(true);
        }

        private async void OnClientDisconnected(object? sender, EventArgs e)
        {
            _consoleService.Warning("Client disconnected. Resetting the state to wait for a new connection...");
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
                _consoleService.WriteColor("Enter a communication type:");
                _consoleService.WriteColor("   0 for Simulation");
                _consoleService.WriteColor("   1 for Physical Biochip");
                _consoleService.WriteEmptyLine(1);
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

            _consoleService.WriteEmptyLine(1);
            _consoleService.Info("Waiting for a client to connect.");
            await _isClientConnected.Task;
            _consoleService.WriteEmptyLine(1);


            return ProgramState.WaitingForUserInput;
        }

        private ProgramState HandleReadInputFiles()
        {
            _programContent = _fileService.ReadFileFromPath(_userService.ProgramPath);

            if (string.IsNullOrEmpty(_programContent)) throw new InvalidOperationException("The uploaded program cannot be empty!");

            _consoleService.WriteColor(_programContent);
            _consoleService.WriteEmptyLine(2);

            // Switch states
            return _configuration.GetValue<bool>("Development:SkipCommunication")
                ? ProgramState.WaitingForUserInput
                : ProgramState.WaitingForClientConnection;
        }


        private async Task<ProgramState> HandleUserInput(CancellationToken token)
        {
            _consoleService.WriteColor("User:");

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
                            _consoleService.WriteColor("Exiting the program...");
                            //await _communicationEngine.StopCommunication()!;
                            Environment.Exit(0);
                            break;

                        case "help":
                        case "?":
                            PrintCommands();
                            return ProgramState.WaitingForUserInput;

                        default:
                            _consoleService.WriteColor("Invalid command. Please try again.");
                            return ProgramState.WaitingForUserInput;
                    }

                    return ProgramState.WaitingForUserInput;
                }
            }

            return _currentState;
        }

        private void PrintCommands()
        {
            _consoleService.WriteColor("Available commands:");
            _consoleService.WriteColor("  start / [Enter]  - Start compiling the program");
            _consoleService.WriteColor("  reupload         - Re-upload the program");
            _consoleService.WriteColor("  disconnect       - Disconnect the client and return to waiting for a new connection");
            _consoleService.WriteColor("  stop / quit / q  - Stop the communication and exit the program");
            _consoleService.WriteEmptyLine(2);
        }
    }
}
