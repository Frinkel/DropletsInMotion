using System.Collections;
using System.Text.Json;
using System.Threading;
using DropletsInMotion.Controllers;
using DropletsInMotion.Compilers;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using DropletsInMotion.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Domain;
using DropletsInMotion.Communication.Simulator.Models;


namespace DropletsInMotion
{
    public class Program
    {
        public static IConfiguration? Configuration { get; private set; }


        static async Task Main(string[] args)
        {
            // Register services in setup
            var serviceProvider = Setup();

            // Get the consoleController
            var consoleController = serviceProvider.GetRequiredService<ConsoleController>();
            consoleController.GetInitialInformation();
            string path = consoleController.ProgramPath;

            // Read file
            string contents = File.ReadAllText(path);
            Console.WriteLine("Input Program:");
            Console.WriteLine(contents);
            


            // Parse the contents
            var inputStream = new AntlrInputStream(contents);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);

            // Get the root of the parse tree (starting with 'program')

            var listener = new MicrofluidicsCustomListener();

            // Walk the parse tree with the custom listener
            var tree = parser.program();
            ParseTreeWalker.Default.Walk(listener, tree);

            CommunicationEngine communicationEngine = new CommunicationEngine(true);
            await communicationEngine.StartCommunication();
            await communicationEngine.WaitForConnection();
            Console.WriteLine("Press any key to start compilation");
            Console.ReadLine();
            Compiler compiler = new Compiler(listener.Droplets, listener.Moves, communicationEngine);
            await compiler.Compile();

            string closeConsole = "";
            while (closeConsole != "q")
            {
                closeConsole = Console.ReadLine() ?? "";
            }
        }


        public static ServiceProvider Setup()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
            serviceCollection.AddSingleton(Configuration);

            serviceCollection.AddTransient<ConsoleController>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}