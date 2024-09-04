using System.Collections;
using System.Text.Json;
using DropletsInMotion.Services.Websocket;
using System.Threading;
using DropletsInMotion.Controllers;
using DropletsInMotion.Compilers;

using DropletsInMotion.Models.Simulator;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Domain;


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

            return serviceCollection.BuildServiceProvider();
        }


        public async static Task StartWebSocket()
        {
            var websocketService = new WebsocketService("http://localhost:5000/ws/");
            var cancellationTokenSource = new CancellationTokenSource();
            var webSocketTask = websocketService.StartServerAsync(cancellationTokenSource.Token);

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Console.WriteLine("Enter a message to send to all connected clients, or 'exit' to stop the server:");
                var input = Console.ReadLine();

                if (input?.ToLower() == "q")
                {
                    cancellationTokenSource.Cancel();
                    break;
                }

                if (!string.IsNullOrWhiteSpace(input))
                {
                    ActionItem actionItem = new ActionItem("electrode", 198, 1);
                    ActionQueueItem actionQueueItem = new ActionQueueItem(actionItem, 0);

                    Queue<ActionQueueItem> actionQueue = new Queue<ActionQueueItem>();

                    actionQueue.Enqueue(actionQueueItem);

                    for (int i = 1; i < 25; i++)
                    {
                        actionItem = new ActionItem("electrode", 198 + i, 1);
                        actionQueueItem = new ActionQueueItem(actionItem, i);
                        actionQueue.Enqueue(actionQueueItem);

                        actionItem = new ActionItem("electrode", 197 + i, 0);
                        actionQueueItem = new ActionQueueItem(actionItem, (decimal)(i + 0.5));
                        actionQueue.Enqueue(actionQueueItem);
                    }

                    WebSocketMessage<Queue<ActionQueueItem>> actionDto =
                        new WebSocketMessage<Queue<ActionQueueItem>>(WebSocketMessageTypes.Action, actionQueue);

                    string serializedObject = JsonSerializer.Serialize(actionDto);
                    await websocketService.SendMessageToAllAsync(serializedObject, cancellationTokenSource.Token);
                }
            }

            await webSocketTask;
        }
    }
}