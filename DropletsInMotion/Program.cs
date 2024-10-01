using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Presentation.Language;
using DropletsInMotion.UI;
using DropletsInMotion.UI.Models;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Communication.Simulator.Services;


namespace DropletsInMotion
{
    public class Program
    {
        private static IConfiguration? _configuration;
        private static IConsoleService? _consoleService;
        private static IFileService? _fileService;

        static async Task Main(string[] args)
        {
            using (var serviceProvider = Setup())
            {
                _consoleService = serviceProvider.GetRequiredService<IConsoleService>();

                try
                {
                    _fileService = serviceProvider.GetRequiredService<IFileService>();

                    // Write the title onto the console
                    string asciiTitle = _fileService.ReadFileFromProjectDirectory("/assets/ascii_title.txt");
                    _consoleService.WriteColor(asciiTitle, ConsoleColor.Blue);
                    _consoleService.WriteEmptyLine(2);

                    StateManager stateManager = serviceProvider.GetRequiredService<StateManager>();
                    await stateManager.Start();
                }
                catch (Exception e)
                {
                    _consoleService.WriteColor(e.Message, ConsoleColor.DarkRed);
                    throw;
                }
               
            }
        }


        

        public static ServiceProvider Setup()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


            _configuration = builder.Build();

            serviceCollection.AddSingleton(_configuration);
            serviceCollection.AddSingleton<IConsoleService, ConsoleService>();
            serviceCollection.AddSingleton<IFileService, FileService>();
            serviceCollection.AddSingleton<ICommunicationService, CommunicationService>();
            serviceCollection.AddSingleton<IWebsocketService, WebsocketService>();
            serviceCollection.AddSingleton<IUserService, UserService>();


            serviceCollection.AddSingleton<StateManager>();
            serviceCollection.AddSingleton<SimulationCommunicationService>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}