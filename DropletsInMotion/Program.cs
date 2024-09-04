using System.Collections;
using System.Text.Json;
using DropletsInMotion.Services.Websocket;
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

            // Output the collected droplets and moves
            Console.WriteLine("Droplets:");
            foreach (var droplet in listener.Droplets)
            {
                Console.WriteLine(droplet);
            }

            Console.WriteLine("Moves:");
            foreach (var move in listener.Moves)
            {
                Console.WriteLine(move);
            }

            Compiler compiler = new Compiler(listener.Droplets, listener.Moves);
            compiler.Compile();
            //await StartWebSocket();
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
            serviceCollection.AddTransient<CommunicationEngine>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}