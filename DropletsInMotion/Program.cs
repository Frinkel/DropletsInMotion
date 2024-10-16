using DropletsInMotion.Application.Execution;
using DropletsInMotion.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Communication.Services;
using DropletsInMotion.UI;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Communication.Simulator.Services;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Presentation;
using DropletsInMotion.Presentation.Services;


namespace DropletsInMotion
{
    public class Program
    {
        private static IConfiguration? _configuration;
        private static IConsoleService? _consoleService;
        private static IFileService? _fileService;
        private static ICommunicationEngine? _communicationEngine;

        static async Task Main(string[] args)
        {
            using (var serviceProvider = Setup())
            {
                _consoleService = serviceProvider.GetRequiredService<IConsoleService>();
                _communicationEngine = serviceProvider.GetRequiredService<ICommunicationEngine>();

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

            // Services
            serviceCollection.AddSingleton(_configuration);
            serviceCollection.AddSingleton<IConsoleService, ConsoleService>();
            serviceCollection.AddSingleton<IFileService, FileService>();
            serviceCollection.AddSingleton<ICommunicationEngine, CommunicationEngine>();
            serviceCollection.AddSingleton<IWebsocketService, WebsocketService>();
            serviceCollection.AddSingleton<IUserService, UserService>();
            serviceCollection.AddSingleton<ITimeService, TimeService>();
            serviceCollection.AddSingleton<IStoreService, StoreService>();
            serviceCollection.AddSingleton<ICommandLifetimeService, CommandLifetimeService>();
            serviceCollection.AddSingleton<ISchedulerService, SchedulerService>();
            serviceCollection.AddSingleton<IContaminationService, ContaminationService>();
            serviceCollection.AddSingleton<IActionService, ActionService>();
            serviceCollection.AddSingleton<IRouterService, RouterService>();
            serviceCollection.AddSingleton<IDependencyService, DependencyService>();
            serviceCollection.AddSingleton<ITemplateService, TemplateService>();
            serviceCollection.AddSingleton<IDependencyBuilder, DependencyBuilder>();
            serviceCollection.AddSingleton<IPlatformService, PlatformService>();
            serviceCollection.AddSingleton<ICommunicationTemplateService, CommunicationTemplateService>();
            serviceCollection.AddSingleton<IDeviceRepository, DeviceRepository>();
            serviceCollection.AddSingleton<ITypeChecker, TypeChecker>();
            serviceCollection.AddSingleton<IPlatformRepository, PlatformRepository>();
            serviceCollection.AddSingleton<ITemplateRepository, TemplateRepository>();


            // Classes
            serviceCollection.AddSingleton<StateManager>();
            serviceCollection.AddSingleton<SimulationCommunicationService>();
            serviceCollection.AddSingleton<ITranslator, Translator>();
            serviceCollection.AddSingleton<IExecutionEngine, ExecutionEngine>();


            return serviceCollection.BuildServiceProvider();
        }
    }
}