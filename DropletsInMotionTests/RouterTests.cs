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
            Debugger.ElapsedTime.Clear();
        }

        [Test]
        public void AStarSearchAroundEachother()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 5, 1);
            var a2 = new Agent("a2", 12, 5, 1);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchAroundEachotherSameSubstance()
        {
            IDropletCommand dropletCommand = new Move("a1", 20, 5);
            IDropletCommand command2 = new Move("a2", 1, 5);
            var commands = new List<IDropletCommand>() { dropletCommand, command2 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 5, 1);
            Agent.ResetSubstanceId();
            var a2 = new Agent("a2", 12, 5, 1);
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
            var a1 = new Agent("a1", 5, 10, 1);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 10, 7, 0, 6);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarSearchSmallWallOfDmf()
        {

            IDropletCommand dropletCommand = new Move("a1", 10, 0);
            var commands = new List<IDropletCommand>() { dropletCommand };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 1);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            _routerService.Initialize(board, 1);

            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 5, 0, 0, 7);

            _ = _routerService.Route(agents, commands, contaminationMap, 0);

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");

            Assert.That(true, Is.EqualTo(IsOneGoalState(commands, agents)));
        }

        [Test]
        public void AStarManyDropletsSimpleRoute()
        {

            IDropletCommand dropletCommandA1 = new Move("a1", 31, 0);
            IDropletCommand dropletCommandA2 = new Move("a2", 31, 5);
            IDropletCommand dropletCommandA3 = new Move("a3", 31, 10);
            IDropletCommand dropletCommandA4 = new Move("a4", 31, 15);
            IDropletCommand dropletCommandA5 = new Move("a5", 31, 19);

            var commands = new List<IDropletCommand>() { dropletCommandA1, dropletCommandA2, dropletCommandA3, dropletCommandA4, dropletCommandA5 };

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 0, 0, 1);
            var a2 = new Agent("a2", 0, 5, 1);
            var a3 = new Agent("a3", 0, 10, 1);
            var a4 = new Agent("a4", 0, 15, 1);
            var a5 = new Agent("a5", 0, 19, 1);

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
            Console.WriteLine($"Average elapsed {Debugger.ElapsedTime.Sum() / Debugger.ElapsedTime.Count}");

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