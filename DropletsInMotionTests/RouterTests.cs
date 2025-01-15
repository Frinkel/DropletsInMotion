using System.Runtime.CompilerServices;
using DropletsInMotion.Application.Factories;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.Services.Routers;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Debugger = DropletsInMotion.Infrastructure.Debugger;

namespace DropletsInMotionTests
{
    public class RouterTests : TestBase
    {

        private readonly ITemplateService _templateService;
        private readonly IContaminationService _contaminationService;
        private readonly IRouterService _routerService;
        private readonly IAgentFactory _agentFactory;
        private readonly IContaminationRepository _contaminationRepository;

        public RouterTests()
        {
            _templateService = ServiceProvider.GetRequiredService<ITemplateService>();
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _routerService = ServiceProvider.GetRequiredService<IRouterService>();
            _agentFactory = ServiceProvider.GetRequiredService<IAgentFactory>();
            _contaminationRepository = ServiceProvider.GetRequiredService<IContaminationRepository>();
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
            Debugger.Permutations = 0;
        }

        [Test]
        public void AStarSearchAroundEachother()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();


            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");


            var a1 = _agentFactory.CreateAgent("a1", 5, 5, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 12, 5, 400, a2Substance);

            agents.Add("a1", a1);
            agents.Add("a2", a2);



            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);
            _contaminationService.ApplyContaminationWithSize(a1, contaminationMap);
            _contaminationService.ApplyContaminationWithSize(a2, contaminationMap);

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");


            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchAroundEachotherExtraDroplet()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            IDropletCommand command3 = new Move("a3", 20, 15);
            var commands = new List<IDropletCommand>() { dropletCommand, command2, command3 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();


            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");
            var a3Substance = _contaminationService.GetSubstanceId("");


            var a1 = _agentFactory.CreateAgent("a1", 5, 5, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 12, 5, 400, a2Substance);
            var a3 = _agentFactory.CreateAgent("a3", 5, 15, 400, a3Substance);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);



            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);
            _contaminationService.ApplyContaminationWithSize(a1, contaminationMap);
            _contaminationService.ApplyContaminationWithSize(a2, contaminationMap);
            _contaminationService.ApplyContaminationWithSize(a3, contaminationMap);

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");


            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchAroundEachotherSameSubstance()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();


            var a1Substance = _contaminationService.GetSubstanceId("Water");
            var a2Substance = _contaminationService.GetSubstanceId("Water");

            var a1 = _agentFactory.CreateAgent("a1", 5, 5, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 12, 5, 400, a2Substance);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            //var a1 = _agentFactory.CreateAgent("a1", 5, 5, 400);
            //Agent.ResetSubstanceId();
            //var a2 = _agentFactory.CreateAgent("a2", 12, 5, 400);
            //agents.Add("a1", a1);
            //agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);
            //_contaminationService.PrintContaminationMap(contaminationMap);

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchGreatWallOfDmf()
        {

            IDropletCommand dropletCommand = new Move("a1", 15, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 5, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 10, 7, 0, 6);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchSmallWallOfDmf()
        {
            IDropletCommand dropletCommand = new Move("a1", 10, 0);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 5, 0, 0, 7);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }



        [Test]
        public void AStarSearchTestCase1()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            IDropletCommand dropletCommand = new Move("a1", 12, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 10, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 1, 0, 18);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();
            

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchHorseShoe()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            IDropletCommand dropletCommand = new Move("a1", 10, 10);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 12, 10, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            //Debugger.Nodes.ForEach(p => Console.Write($"({p.x} {p.y}) "));
            //Debugger.PrintDuplicateCounts();


            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void AStarTheMaze()
        {
            //Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

            IDropletCommand dropletCommand = new Move("a1", 0, 0);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 31, 19, 400);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 7, 2, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 15, 2, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 19, 0, 0, 18);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 23, 2, 0, 18);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

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
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 400);
            var a2 = _agentFactory.CreateAgent("a2", 0, 5, 400);
            var a3 = _agentFactory.CreateAgent("a3", 0, 10, 400);
            var a4 = _agentFactory.CreateAgent("a4", 0, 15, 400);
            var a5 = _agentFactory.CreateAgent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 400);
            var a2 = _agentFactory.CreateAgent("a2", 0, 5, 400);
            var a3 = _agentFactory.CreateAgent("a3", 0, 10, 400);
            var a4 = _agentFactory.CreateAgent("a4", 0, 15, 400);
            var a5 = _agentFactory.CreateAgent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 400);
            var a2 = _agentFactory.CreateAgent("a2", 0, 5, 400);
            var a3 = _agentFactory.CreateAgent("a3", 0, 10, 400);
            var a4 = _agentFactory.CreateAgent("a4", 0, 15, 400);
            var a5 = _agentFactory.CreateAgent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
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
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 400);
            var a2 = _agentFactory.CreateAgent("a2", 0, 5, 400);
            var a3 = _agentFactory.CreateAgent("a3", 0, 10, 400);
            var a4 = _agentFactory.CreateAgent("a4", 0, 15, 400);
            var a5 = _agentFactory.CreateAgent("a5", 0, 19, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
            var a1 = _agentFactory.CreateAgent("a1", 10, 1, 400);
            var a2 = _agentFactory.CreateAgent("a2", 10, 5, 400);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void AStarTwoDropletsSameSubstanceOneBigOneSmall()
        {

            IDropletCommand dropletCommandA2 = new Move("a2", 5, 7);

            var commands = new List<IDropletCommand>() {  dropletCommandA2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 8, 5, 2827, 1);
            var a2 = _agentFactory.CreateAgent("a2", 15, 7, 706, 1);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContaminationWithSize(a1, contaminationMap);
            _contaminationService.ApplyContaminationWithSize(a2, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }


        [Test]
        public void AStarFourDropletsCrissCross()
        {
            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }


            IDropletCommand dropletCommandA1 = new Move("a1", 15, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);

            var commands = new List<IDropletCommand>() {  dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA1 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();


            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");
            var a3Substance = _contaminationService.GetSubstanceId("");
            var a4Substance = _contaminationService.GetSubstanceId("");


            var a1 = _agentFactory.CreateAgent("a1", 9, 7, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 11, 7, 400, a2Substance);
            var a3 = _agentFactory.CreateAgent("a3", 13, 7, 400, a3Substance);
            var a4 = _agentFactory.CreateAgent("a4", 15, 7, 400, a4Substance);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarFiveDropletsCrissCross()
        {
            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }


            IDropletCommand dropletCommandA1 = new Move("a1", 17, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);
            IDropletCommand dropletCommandA5 = new Move("a4", 15, 11);

            var commands = new List<IDropletCommand>() { dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA1, dropletCommandA5 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();


            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");
            var a3Substance = _contaminationService.GetSubstanceId("");
            var a4Substance = _contaminationService.GetSubstanceId("");
            var a5Substance = _contaminationService.GetSubstanceId("");


            var a1 = _agentFactory.CreateAgent("a1", 9, 7, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 11, 7, 400, a2Substance);
            var a3 = _agentFactory.CreateAgent("a3", 13, 7, 400, a3Substance);
            var a4 = _agentFactory.CreateAgent("a4", 15, 7, 400, a4Substance);
            var a5 = _agentFactory.CreateAgent("a5", 17, 7, 400, a5Substance);

            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
            //Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");
            //Debugger.PrintDuplicateCounts();
            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSevenDropletsCrissCross()
        {

            var process = Debugger.GetProcess();

            Debugger.PrintMemoryUsage(process);

            //process.Refresh();
            //Console.WriteLine($"mem: {process.WorkingSet64 / (1024.0 * 1024.0)}");

            IDropletCommand dropletCommandA1 = new Move("a1", 21, 11);
            IDropletCommand dropletCommandA2 = new Move("a2", 9, 11);
            IDropletCommand dropletCommandA3 = new Move("a3", 11, 11);
            IDropletCommand dropletCommandA4 = new Move("a4", 13, 11);
            IDropletCommand dropletCommandA5 = new Move("a5", 15, 11);
            IDropletCommand dropletCommandA6 = new Move("a6", 17, 11);
            IDropletCommand dropletCommandA7 = new Move("a7", 19, 11);


            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA5, dropletCommandA6, dropletCommandA7 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");
            var a3Substance = _contaminationService.GetSubstanceId("");
            var a4Substance = _contaminationService.GetSubstanceId("");
            var a5Substance = _contaminationService.GetSubstanceId("");
            var a6Substance = _contaminationService.GetSubstanceId("");
            var a7Substance = _contaminationService.GetSubstanceId("");

            var a1 = _agentFactory.CreateAgent("a1", 9, 7, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 11, 7, 400, a2Substance);
            var a3 = _agentFactory.CreateAgent("a3", 13, 7, 400, a3Substance);
            var a4 = _agentFactory.CreateAgent("a4", 15, 7, 400, a4Substance);
            var a5 = _agentFactory.CreateAgent("a5", 17, 7, 400, a5Substance);
            var a6 = _agentFactory.CreateAgent("a6", 19, 7, 400, a6Substance);
            var a7 = _agentFactory.CreateAgent("a7", 21, 7, 400, a7Substance);


            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);
            agents.Add("a6", a6);
            agents.Add("a7", a7);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);
            _contaminationService.ApplyContamination(a6, contaminationMap);
            _contaminationService.ApplyContamination(a7, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Debugger.PrintMemoryUsage(process);
            //process.Refresh();
            //Console.WriteLine($"mem: {process.WorkingSet64 / (1024.0 * 1024.0)}");
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
            var a1 = _agentFactory.CreateAgent("a1", 1, 0, 400);
            var a2 = _agentFactory.CreateAgent("a2", 3, 0, 400);
            var a3 = _agentFactory.CreateAgent("a3", 5, 0, 400);
            var a4 = _agentFactory.CreateAgent("a4", 7, 0, 400);
            var a5 = _agentFactory.CreateAgent("a5", 9, 0, 400);
            var a6 = _agentFactory.CreateAgent("a6", 11, 0, 400);
            var a7 = _agentFactory.CreateAgent("a7", 13, 0, 400);
            var a8 = _agentFactory.CreateAgent("a8", 15, 0, 400);
            var a9 = _agentFactory.CreateAgent("a9", 17, 0, 400);
            var a10 = _agentFactory.CreateAgent("a10", 19, 0, 400);


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
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

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

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
            var a1 = _agentFactory.CreateAgent("a1", 6, 10, 400);
            //Agent.ResetSubstanceId();
            var a2 = _agentFactory.CreateAgent("a2", 10, 8, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

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
            var a1 = _agentFactory.CreateAgent("a1", 4, 5, 400);
            //Agent.ResetSubstanceId();
            var a2 = _agentFactory.CreateAgent("a2", 10, 3, 400);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 11, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 15, 8, 0);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 5, 0, 10);
            //_contaminationService.UpdateContaminationArea(contaminationMap, 255, 3, 11, 0, 4);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

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
            var a1Substance = _contaminationService.GetSubstanceId("");
            var a2Substance = _contaminationService.GetSubstanceId("");
            var a3Substance = _contaminationService.GetSubstanceId("");
            var a4Substance = _contaminationService.GetSubstanceId("");
            var a5Substance = _contaminationService.GetSubstanceId("");
            var a6Substance = _contaminationService.GetSubstanceId("");

            var a1 = _agentFactory.CreateAgent("a1", 9, 7, 400, a1Substance);
            var a2 = _agentFactory.CreateAgent("a2", 11, 7, 400, a2Substance);
            var a3 = _agentFactory.CreateAgent("a3", 13, 7, 400, a3Substance);
            var a4 = _agentFactory.CreateAgent("a4", 15, 7, 400, a4Substance);
            var a5 = _agentFactory.CreateAgent("a5", 17, 7, 400, a5Substance);
            var a6 = _agentFactory.CreateAgent("a6", 19, 7, 400, a6Substance);


            agents.Add("a1", a1);
            agents.Add("a2", a2);
            agents.Add("a3", a3);
            agents.Add("a4", a4);
            agents.Add("a5", a5);
            agents.Add("a6", a6);

            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.ApplyContamination(a2, contaminationMap);
            _contaminationService.ApplyContamination(a3, contaminationMap);
            _contaminationService.ApplyContamination(a4, contaminationMap);
            _contaminationService.ApplyContamination(a5, contaminationMap);
            _contaminationService.ApplyContamination(a6, contaminationMap);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");
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
                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j, 0, 0);
                }
            }
            return board;
        }
    }
}