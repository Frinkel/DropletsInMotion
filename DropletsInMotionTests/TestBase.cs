using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Communication.Simulator.Services;
using DropletsInMotion.Communication;
using DropletsInMotion.Communication.Services;
using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.UI;
using Microsoft.Extensions.Configuration;
using DropletsInMotion.Presentation.Services;
using DropletsInMotion.Presentation;

namespace DropletsInMotionTests
{
    public abstract class TestBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected TestBase()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


            IConfiguration _configuration = builder.Build();

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


            // Classes
            serviceCollection.AddSingleton<StateManager>();
            serviceCollection.AddSingleton<SimulationCommunicationService>();
            serviceCollection.AddSingleton<ITranslator, Translator>();
            serviceCollection.AddSingleton<IExecutionEngine, ExecutionEngine>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
