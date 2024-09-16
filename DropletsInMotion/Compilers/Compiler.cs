using DropletsInMotion.Communication;
using DropletsInMotion.Domain;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Compilers.Services;
using DropletsInMotion.Controllers;
using DropletsInMotion.Controllers.ConsoleController;
using DropletsInMotion.Routers;


namespace DropletsInMotion.Compilers
{
    public class Compiler
    {
        public CommunicationEngine CommunicationEngine;

        public Electrode[][] Board { get; set; }

        public Dictionary<string, Droplet> Droplets { get; set; } = new Dictionary<string, Droplet>();

        public double time = 0;

        private TemplateHandler TemplateHandler;

        private PlatformService PlatformService;

        private DependencyGraph DependencyGraph;

        private Router Router;

        public Compiler(List<ICommand> commands, Dictionary<string, Droplet> droplets, CommunicationEngine communicationEngine, string platformPath)
        {
            CommunicationEngine = communicationEngine;

            PlatformService = new PlatformService(platformPath);

            Board = PlatformService.Board;

            Console.WriteLine(Board[0][1]);
            TemplateHandler = new TemplateHandler(Board);

            DependencyGraph = new DependencyGraph(commands);

            DependencyGraph.PrintGraph();

            Router = new Router(Board);

            Droplets = droplets;

        }

        public async Task Compile()
        {

            List<BoardAction> boardActions = new List<BoardAction>();
            int i = 0;
            while (DependencyGraph.GetExecutableNodes().Count > 0)
            {
                List<DependencyNode> executableNodes = DependencyGraph.GetExecutableNodes();
                List<ICommand> commandsToExecute = executableNodes.ConvertAll(node => node.Command);
                //print the commands
                Console.WriteLine($"Commands to execute iteration {i}:");
                foreach (ICommand command in commandsToExecute)
                {
                    Console.WriteLine(command);
                }

                boardActions.AddRange(Router.Route(Droplets, commandsToExecute, time));
                boardActions = boardActions.OrderBy(b => b.Time).ToList();
                time = boardActions.Any() ? boardActions.Last().Time : time;
                
                //Console.WriteLine("\nPRINTING ACTIONS\n");
                //foreach (var action in boardActions)
                //{
                //    Console.WriteLine(action.ToString());
                //}

                foreach (DependencyNode node in executableNodes)
                {
                    DependencyGraph.MarkNodeAsExecuted(node.NodeId);
                }
                i += 1;

                await CommunicationEngine.SendActions(boardActions);
                boardActions.Clear();

            }


        }

    }
}
