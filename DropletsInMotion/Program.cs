using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Presentation.Language;
using DropletsInMotion.UI;
using DropletsInMotion.UI.Models;


namespace DropletsInMotion
{
    public class Program
    {
        public static IConfiguration? Configuration { get; private set; }
        public static CommunicationEngine? CommunicationEngine { get; private set; }

        static async Task Main(string[] args)
        {
            // Register services in setup
            var serviceProvider = Setup();

            // Get the consoleController    
            var consoleController = serviceProvider.GetRequiredService<IConsoleService>();


            // Title
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? "";
            string asciiTitle = File.ReadAllText(projectDirectory + "/assets/ascii_title.txt");
            consoleController.WriteColor(asciiTitle, ConsoleColor.Blue);
            Console.WriteLine();

            // Show commands
            PrintCommands();

            // Start the communication
            Console.WriteLine("Booting up communication engine..");
            CommunicationEngine = new CommunicationEngine(true); // TODO: We force it to be simulation, we need to make it user selected.
            await CommunicationEngine.StartCommunication();

            // Set the current state
            ProgramState currentState = ProgramState.GettingInitialInformation;

            // "Global" variables
            string path = "";
            string programContent = "";

            
            Console.WriteLine();

            while (true)
            {
                try
                {
                    switch (currentState)
                    {
                        case (ProgramState.GettingInitialInformation):
                            consoleController.GetInitialInformation();
                            path = consoleController.ProgramPath;

                            currentState = ProgramState.ReadingInputFiles;
                            break;
                        case ProgramState.ReadingInputFiles:

                            // TODO: Maybe we need a filereader here to also handle configuration?
                            programContent = File.ReadAllText(path);
                            Console.WriteLine("Program:");
                            Console.WriteLine(programContent);
                            //consoleController.WriteColor(programContent, ConsoleColor.Black, ConsoleColor.DarkCyan);
                            Console.WriteLine();

                            if (Configuration != null && Configuration.GetValue<bool>("Development:SkipCommunication"))
                            {
                                currentState = ProgramState.CompilingProgram;
                            }
                            else
                            {
                                currentState = ProgramState.WaitingForClientConnection;
                            }
                            
                            break;
                        case (ProgramState.WaitingForClientConnection):

                            if (await CommunicationEngine.IsClientConnected()) { currentState = ProgramState.WaitingForUserInput; break; }

                            Console.WriteLine("Waiting for a client to connect...");
                            await CommunicationEngine.WaitForConnection();
                            await Task.Delay(100);
                            Console.WriteLine();

                            currentState = ProgramState.WaitingForUserInput;
                            break;

                        case (ProgramState.WaitingForUserInput):
                            // TODO: Maybe we can add more commands here? Reupload program, Quit, Disconnect client, etc.
                            Console.Write("User input: ");
                            currentState = await HandleUserInput();
                            break;

                        case (ProgramState.CompilingProgram):
                            //Console.WriteLine("Press the \"ENTER\" key to start the compilation");
                            //Console.ReadLine();

                            // Parse the contents
                            var inputStream = new AntlrInputStream(programContent);
                            var lexer = new MicrofluidicsLexer(inputStream);
                            var commonTokenStream = new CommonTokenStream(lexer);
                            var parser = new MicrofluidicsParser(commonTokenStream);
                            var listener = new MicrofluidicsCustomListener();
                            var tree = parser.program();
                            ParseTreeWalker.Default.Walk(listener, tree);
                            foreach (var elem in listener.Commands)
                            {
                                Console.WriteLine(elem.ToString());
                            }

                            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, CommunicationEngine,
                                consoleController.PlatformPath);
                            await compiler.Compile();


                            // Compile the program
                            //Compiler compiler = new Compiler(listener.Droplets, listener.Moves, CommunicationEngine, consoleController.PlatformPath);
                            //await compiler.Compile();
                            // TODO: Maybe the compiler should return a status code for the compilation.

                            currentState = ProgramState.Completed;
                            break;

                        case (ProgramState.Completed):
                            // Finish
                            consoleController.WriteSuccess("\nProgram compiled successfully!\n");

                            currentState = ProgramState.WaitingForUserInput;
                            break;

                        default:
                            throw new Exception("A non-reachable state was reached");
                    }
                }
                catch (Exception e)
                {
                    consoleController.WriteColor(e.Message, ConsoleColor.DarkRed);
                    Console.WriteLine("Stopping communication channels...");
                    await CommunicationEngine.StopCommunication();
                    throw;
                }
            }
        }


        private static async Task<ProgramState> HandleUserInput()
        {
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
                    await CommunicationEngine?.StopCommunication()!;
                    return ProgramState.WaitingForClientConnection;

                case "stop":
                case "quit":
                case "q":
                    Console.WriteLine("Exiting the program...");
                    await CommunicationEngine?.StopCommunication()!;
                    Environment.Exit(0);
                    break;

                case "help":
                case "?":
                    PrintCommands();
                    return ProgramState.WaitingForUserInput;

                default:
                    Console.WriteLine("Invalid command. Please try again.");
                    return ProgramState.WaitingForUserInput;
            }

            return ProgramState.WaitingForUserInput;
        }

        private static void PrintCommands()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  start / [Enter]  - Start compiling the program");
            Console.WriteLine("  reupload         - Re-upload the program");
            Console.WriteLine("  disconnect       - Disconnect the client and return to waiting for a new connection");
            Console.WriteLine("  stop / quit / q  - Stop the communication and exit the program");
            Console.WriteLine();
        }

        public static ServiceProvider Setup()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


            Configuration = builder.Build();

            serviceCollection.AddSingleton(Configuration);
            serviceCollection.AddSingleton<IConsoleService, ConsoleService>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}