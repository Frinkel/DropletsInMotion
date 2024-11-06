using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class RouterTests : TestBase
    {

        private readonly ITemplateService _templateService;
        private readonly IContaminationService _contaminationService;
        private readonly IRouterService _routerService;

        public RouterTests()
        {
            _templateService = ServiceProvider.GetRequiredService<ITemplateService>();
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _routerService = ServiceProvider.GetRequiredService<IRouterService>();
        }

        [SetUp]
        public void Setup()
        {
            Agent.ResetSubstanceId();
            Debugger.ExpandedStates = 0;
            Debugger.ExistingStates = 0;
            Debugger.ExploredStates = 0;
            Debugger.Nodes = new List<(int x, int y)>();
            Debugger.ElapsedTime.Clear();
        }

        [Test]
        public void AStarSearchAroundEachother()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 5, 400);
            var a2 = new Agent("a2", 12, 5, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];
            _contaminationService.ApplyContaminationWithSize(a1, contaminationMap);
            _contaminationService.ApplyContaminationWithSize(a2, contaminationMap);

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchAroundEachotherSameSubstance()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 5, 400);
            Agent.ResetSubstanceId();
            var a2 = new Agent("a2", 12, 5, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchGreatWallOfDmf()
        {

            IDropletCommand dropletCommand = new Move("a1", 15, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 10, 7, 0, 6);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchSmallWallOfDmf()
        {
            IDropletCommand dropletCommand = new Move("a1", 10, 0);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 5, 0, 0, 7);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }



        [Test]
        public void AStarSearchTestCase1()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            IDropletCommand dropletCommand = new Move("a1", 12, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 10, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 1, 0, 18);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();
            

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchHorseShoe()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            IDropletCommand dropletCommand = new Move("a1", 10, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 12, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();


            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void AStarTheMaze()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            IDropletCommand dropletCommand = new Move("a1", 0, 0);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 31, 19, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 7, 2, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 15, 2, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 19, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 23, 2, 0, 18);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();


            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void XAStarManyDropletsSimpleRoute()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 31, 0);
            IDropletCommand dropletCommandA2 = new Move("a2", 31, 5);
            IDropletCommand dropletCommandA3 = new Move("a3", 31, 10);
            IDropletCommand dropletCommandA4 = new Move("a4", 31, 15);
            IDropletCommand dropletCommandA5 = new Move("a5", 31, 19);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA5 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 400);
            var a2 = new Agent("a2", 0, 5, 400);
            var a3 = new Agent("a3", 0, 10, 400);
            var a4 = new Agent("a4", 0, 15, 400);
            var a5 = new Agent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void XAStarTwoDropletsSimpleRoute()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 31, 0);
            IDropletCommand dropletCommandA2 = new Move("a2", 31, 5);
            IDropletCommand dropletCommandA3 = new Move("a3", 31, 10);
            IDropletCommand dropletCommandA4 = new Move("a4", 31, 15);
            IDropletCommand dropletCommandA5 = new Move("a5", 31, 19);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 400);
            var a2 = new Agent("a2", 0, 5, 400);
            var a3 = new Agent("a3", 0, 10, 400);
            var a4 = new Agent("a4", 0, 15, 400);
            var a5 = new Agent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void XAStarThreeDropletsSimpleRoute()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 31, 0);
            IDropletCommand dropletCommandA2 = new Move("a2", 31, 5);
            IDropletCommand dropletCommandA3 = new Move("a3", 31, 10);
            IDropletCommand dropletCommandA4 = new Move("a4", 31, 15);
            IDropletCommand dropletCommandA5 = new Move("a5", 31, 19);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 400);
            var a2 = new Agent("a2", 0, 5, 400);
            var a3 = new Agent("a3", 0, 10, 400);
            var a4 = new Agent("a4", 0, 15, 400);
            var a5 = new Agent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void XAStarFourDropletsSimpleRoute()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 31, 0);
            IDropletCommand dropletCommandA2 = new Move("a2", 31, 5);
            IDropletCommand dropletCommandA3 = new Move("a3", 31, 10);
            IDropletCommand dropletCommandA4 = new Move("a4", 31, 15);
            IDropletCommand dropletCommandA5 = new Move("a5", 31, 19);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 400);
            var a2 = new Agent("a2", 0, 5, 400);
            var a3 = new Agent("a3", 0, 10, 400);
            var a4 = new Agent("a4", 0, 15, 400);
            var a5 = new Agent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarTwoDropletsBenchmark()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 20, 5);
            IDropletCommand dropletCommandA2 = new Move("a2", 20, 1);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 10, 1, 400);
            var a2 = new Agent("a2", 10, 5, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void AStarFourDropletsCrissCross()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 15, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);

            var commands = new List<IDropletCommand>() {  dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA1 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 9, 7, 400);
            var a2 = new Agent("a2", 11, 7, 400);
            var a3 = new Agent("a3", 13, 7, 400);
            var a4 = new Agent("a4", 15, 7, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSevenDropletsCrissCross()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 21, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);
            IDropletCommand dropletCommandA5 = new Move("a5", 15, 11);
            IDropletCommand dropletCommandA6 = new Move("a6", 17, 11);
            IDropletCommand dropletCommandA7 = new Move("a7", 19, 11);


            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA5, dropletCommandA6, dropletCommandA7 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 9, 7, 400);
            var a2 = new Agent("a2", 11, 7, 400);
            var a3 = new Agent("a3", 13, 7, 400);
            var a4 = new Agent("a4", 15, 7, 400);
            var a5 = new Agent("a5", 17, 7, 400);
            var a6 = new Agent("a6", 19, 7, 400);
            var a7 = new Agent("a7", 21, 7, 400);


            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);
            agents.Add("a6", a6);
            agents.Add("a7", a7);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);
            _contaminationService.ApplyContamination(a6, contaminationMap);
            _contaminationService.ApplyContamination(a7, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarTenDroplets()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 1, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 3, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 5, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 7, 11);
            IDropletCommand dropletCommandA5 = new Move("a5", 9, 11);
            IDropletCommand dropletCommandA6 = new Move("a6", 11, 11);
            IDropletCommand dropletCommandA7 = new Move("a7", 13, 11);
            IDropletCommand dropletCommandA8 = new Move("a8", 15, 11);
            IDropletCommand dropletCommandA9 = new Move("a9", 17, 11);
            IDropletCommand dropletCommandA10 = new Move("a10", 19, 11);


            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA5, dropletCommandA6, dropletCommandA7, dropletCommandA8, dropletCommandA9, dropletCommandA10 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 1, 0, 400);
            var a2 = new Agent("a2", 3, 0, 400);
            var a3 = new Agent("a3", 5, 0, 400);
            var a4 = new Agent("a4", 7, 0, 400);
            var a5 = new Agent("a5", 9, 0, 400);
            var a6 = new Agent("a6", 11, 0, 400);
            var a7 = new Agent("a7", 13, 0, 400);
            var a8 = new Agent("a8", 15, 0, 400);
            var a9 = new Agent("a9", 17, 0, 400);
            var a10 = new Agent("a10", 19, 0, 400);


            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);
            agents.Add("a6", a6);
            agents.Add("a7", a7);
            agents.Add("a8", a8);
            agents.Add("a9", a9);
            agents.Add("a10", a10);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);
            _contaminationService.ApplyContamination(a6, contaminationMap);
            _contaminationService.ApplyContamination(a7, contaminationMap);
            _contaminationService.ApplyContamination(a8, contaminationMap);
            _contaminationService.ApplyContamination(a9, contaminationMap);
            _contaminationService.ApplyContamination(a10, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarNewTest()
        {

            IDropletCommand dropletCommand = new Move("a1", 14, 10);
            IDropletCommand dropletCommand2 = new Move("a2", 10, 19);
            var commands = new List<IDropletCommand>() { dropletCommand, dropletCommand2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 6, 10, 400);
            //Agent.ResetSubstanceId();
            var a2 = new Agent("a2", 10, 8, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();


            Assert.That(IsOneGoalState(commands, agents), Is.EqualTo(true));
        }
        [Test]
        public void AStarNewTest2()
        {

            IDropletCommand dropletCommand = new Move("a1", 8, 5);
            IDropletCommand dropletCommand2 = new Move("a2", 6, 7);
            var commands = new List<IDropletCommand>() { dropletCommand2, dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 4, 5, 400);
            //Agent.ResetSubstanceId();
            var a2 = new Agent("a2", 10, 3, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();


            Assert.That(IsOneGoalState(commands, agents), Is.EqualTo(true));
        }

        [Test]
        public void AStarSixDropletsCrissCross()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 19, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);
            IDropletCommand dropletCommandA5 = new Move("a5", 15, 11);
            IDropletCommand dropletCommandA6 = new Move("a6", 17, 11);


            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA6, dropletCommandA5, dropletCommandA4 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 9, 7, 400);
            var a2 = new Agent("a2", 11, 7, 400);
            var a3 = new Agent("a3", 13, 7, 400);
            var a4 = new Agent("a4", 15, 7, 400);
            var a5 = new Agent("a5", 17, 7, 400);
            var a6 = new Agent("a6", 19, 7, 400);


            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);
            agents.Add("a6", a6);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);
            _contaminationService.ApplyContamination(a6, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        public bool IsOneGoalState(List<IDropletCommand> commands, Dictionary<string, Agent> droplets)
        {
            foreach (var dropletCommand in commands)
            {
                Agent agent;
                switch (dropletCommand)
                {
                    case Move moveCommand:
                        agent = droplets[moveCommand.GetInputDroplets().First()];
                        if (agent.PositionX == moveCommand.PositionX && agent.PositionY == moveCommand.PositionY)
                            return true;
                        break;
                    default:
                        return false;
                        break;
                }
            }

            return false;
        }

        public Electrode[][] CreateBoard()
        {
            Electrode[][] board = new Electrode[32][];
            board = new Electrode[32][];
            for (int i = 0; i < 32; i++)
            {
                board[i] = new Electrode[20];
                for (int j = 0; j < 20; j++)
                {
                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
                }
            }
            return board;
        }
    }
}