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
        public List<Droplet> Droplets { get; } = new List<Droplet>(); 
        public double time = 0;

        private TemplateHandler TemplateHandler;

        private PlatformService PlatformService;

        private DependencyGraph DependencyGraph;

        private Router Router;

        public Compiler(List<ICommand> commands, List<Droplet> droplets, CommunicationEngine communicationEngine, string platformPath)
        {
            CommunicationEngine = communicationEngine;

            PlatformService = new PlatformService(platformPath);

            Board = PlatformService.Board;

            Console.WriteLine(Board[0][1]);
            TemplateHandler = new TemplateHandler(Board);

            DependencyGraph = new DependencyGraph(commands);

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
                //Console.WriteLine($"Commands to execute iteration {i}:");
                //foreach (ICommand command in commandsToExecute)
                //{
                //    Console.WriteLine(command);
                //}

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

        //public List<BoardAction> CompileMove(Move move, double compileTime)
        //{
        //    List<BoardAction> boardActions = new List<BoardAction>();
        //    string dropletName = move.DropletName;
        //    Droplet droplet = Droplets.Find(d => d.DropletName == dropletName);

        //    if (droplet == null)
        //    {
        //        throw new InvalidOperationException($"Droplet with name {dropletName} not found.");
        //    }

        //    int currentX = droplet.PositionX;
        //    int currentY = droplet.PositionY;
        //    int targetX = move.NewPositionX;
        //    int targetY = move.NewPositionY;

        //    double time = compileTime;

        //    // Move horizontally first (if needed)
        //    while (currentX != targetX)
        //    {
        //        if (currentX < targetX)
        //        {
        //            currentX++;
        //            List<BoardAction> appliedMove = TemplateHandler.ApplyTemplate("moveRight", droplet, time);
        //            boardActions.AddRange(appliedMove);
        //            droplet.PositionX = currentX;
        //            time = appliedMove.Last().Time;
        //        }
        //        else
        //        {

        //            currentX--;
        //            List<BoardAction> appliedMove = TemplateHandler.ApplyTemplate("moveLeft", droplet, time);
        //            boardActions.AddRange(appliedMove);
        //            droplet.PositionX = currentX;
        //            time = appliedMove.Last().Time;
        //        }


        //    }

        //    // Move vertically (if needed)
        //    while (currentY != targetY)
        //    {
        //        if (currentY < targetY)
        //        {
        //            currentY++;
        //            List<BoardAction> appliedMove = TemplateHandler.ApplyTemplate("moveDown", droplet, time);
        //            boardActions.AddRange(appliedMove);
        //            droplet.PositionY = currentY;
        //            time = appliedMove.Last().Time;
        //        }
        //        else
        //        {
        //            currentY--;
        //            List<BoardAction> appliedMove = TemplateHandler.ApplyTemplate("moveUp", droplet, time);
        //            boardActions.AddRange(appliedMove);
        //            droplet.PositionY = currentY;
        //            time = appliedMove.Last().Time;
        //        }
        //    }

        //    // Update the droplet's final position
        //    droplet.PositionX = targetX;
        //    droplet.PositionY = targetY;

        //    foreach (var action in boardActions)
        //    {
        //        Console.WriteLine(action);
        //    }
        //    return boardActions;
        //}




    }
}
