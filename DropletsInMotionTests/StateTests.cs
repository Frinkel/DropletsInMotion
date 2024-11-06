using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Execution;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;

namespace DropletsInMotionTests
{
    public class StateTests : TestBase
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IContaminationService _contaminationService;
        private readonly IPlatformRepository _platformRepository;
        public StateTests()
        {
            _templateRepository = ServiceProvider.GetRequiredService<ITemplateRepository>();
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _platformRepository = ServiceProvider.GetRequiredService<IPlatformRepository>();
        }


        //[Test]
        //public void CalculateHeuristic()
        //{
        //    byte[,] contamination = new byte[32, 20];


        //    Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
        //    var agent = new Agent("d1", 1, 1, 1);
        //    agents.Add("d1", agent);
        //    IDropletCommand dropletCommand = new Move("d1", 3, 3);
        //    List<IDropletCommand> commands = new List<IDropletCommand>() { dropletCommand };
        //    List<string> routeableAgents = new List<string>() { "d1" };

        //    State s1 = new State(routeableAgents, agents, contamination, commands,  _contaminationService, _platformRepository, _templateRepository);

        //    Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
        //    jointAction.Add("d1", Types.RouteAction.MoveRight);

        //    State s2 = new State(s1, jointAction);
        //    Assert.That(s2.GetHeuristic(), Is.EqualTo(7));
        //    State s3 = new State(s2, jointAction);
        //    Assert.That(s3.GetHeuristic(), Is.EqualTo(6));
        //}

        //[Test]
        //public void ExtractActionsTest()
        //{
        //    byte[,] contamination = new byte[32, 20];


        //    var agents = CreateTwoAgentsWithPositions(1, 1, 5, 5);

        //    IDropletCommand command1 = new Move("d1", 3, 3);
        //    IDropletCommand command2 = new Move("d2", 7, 7);

        //    List<IDropletCommand> commands = new List<IDropletCommand>() { command1, command2 };
        //    List<string> routeableAgents = new List<string>() { "d1", "d2" };


        //    Electrode[][] board = new Electrode[32][];
        //    for (int i = 0; i < 32; i++)
        //    {
        //        board[i] = new Electrode[20];
        //        for (int j = 0; j < 20; j++)
        //        {
        //            board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
        //        }
        //    }
        //    State s1 = new State(routeableAgents, agents, contamination, commands, _contaminationService, _platformRepository, _templateRepository);

        //    Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
        //    jointAction.Add("d1", Types.RouteAction.MoveRight);
        //    jointAction.Add("d2", Types.RouteAction.MoveRight);
        //    State s2 = new State(s1, jointAction);
        //    State s3 = new State(s2, jointAction);

        //    Assert.That(s3.ExtractActions(0).Count, Is.EqualTo(12));
        //}

        //[Test]
        //public void TestExpandedStates()
        //{
        //    byte[,] contamination = new byte[32, 20];

        //    var agents = CreateTwoAgentsWithPositions(1, 1, 5, 5);
        //    IDropletCommand command1 = new Move("d1", 3, 3);
        //    IDropletCommand command2 = new Move("d2", 7, 7);

        //    List<IDropletCommand> commands = new List<IDropletCommand>() { command1, command2 };
        //    List<string> routeableAgents = new List<string>() { "d1", "d2" };

        //    State s1 = new State(routeableAgents, agents, contamination, commands, _contaminationService, _platformRepository, _templateRepository);

        //    List<State> expandedStates = s1.GetExpandedStates();

        //    Assert.That(expandedStates.Count(), Is.EqualTo(25));
        //}

        //[Test]
        //public void TestIsMoveApplicable()
        //{
        //    byte[,] contamination = new byte[32, 20];

        //    var agents = CreateTwoAgentsWithPositions(1, 1, 5, 1);

        //    IDropletCommand command1 = new Move("d1", 3, 3);
        //    IDropletCommand command2 = new Move("d2", 7, 7);

        //    List<IDropletCommand> commands = new List<IDropletCommand>() { command1, command2 };
        //    List<string> routeableAgents = new List<string>() { "d1", "d2" };

        //    State s1 = new State(routeableAgents, agents, contamination, commands, _contaminationService, _platformRepository, _templateRepository);

        //    Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
        //    jointAction.Add("d1", Types.RouteAction.MoveRight);
        //    jointAction.Add("d2", Types.RouteAction.MoveLeft);
        //    List<State> expandedStates = s1.GetExpandedStates();
        //    Assert.That(expandedStates.Count(), Is.EqualTo(25));

        //    State s2 = new State(s1, jointAction);
        //    expandedStates = s2.GetExpandedStates();
        //    Assert.That(expandedStates.Count(), Is.EqualTo(9));
        //}

        //[Test]
        //public void TestIsConflicting()
        //{
        //    byte[,] contamination = new byte[32, 20];


        //    var agents = CreateTwoAgentsWithPositions(1, 1, 6, 1);

        //    IDropletCommand command1 = new Move("d1", 3, 3);
        //    IDropletCommand command2 = new Move("d2", 7, 7);

        //    List<IDropletCommand> commands = new List<IDropletCommand>() { command1, command2 };
        //    List<string> routeableAgents = new List<string>() { "d1", "d2" };

        //    State s1 = new State(routeableAgents, agents, contamination, commands, _contaminationService, _platformRepository, _templateRepository);

        //    Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
        //    jointAction.Add("d1", Types.RouteAction.MoveRight);
        //    jointAction.Add("d2", Types.RouteAction.MoveLeft);
        //    List<State> expandedStates = s1.GetExpandedStates();
        //    Assert.That(expandedStates.Count(), Is.EqualTo(25));

        //    State s2 = new State(s1, jointAction);
        //    expandedStates = s2.GetExpandedStates();
        //    Assert.That(expandedStates.Count(), Is.EqualTo(15));
        //}

        //[Test]

        //public void TestIsGoalState()
        //{
        //    byte[,] contamination = new byte[32, 20];
        //    var agents = CreateTwoAgentsWithPositions(1, 3, 5, 7);

        //    IDropletCommand command1 = new Move("d1", 3, 3);
        //    IDropletCommand command2 = new Move("d2", 7, 7);

        //    List<IDropletCommand> commands = new List<IDropletCommand>() { command1, command2 };
        //    List<string> routeableAgents = new List<string>() { "d1", "d2" };

        //    State s1 = new State(routeableAgents, agents, contamination, commands, _contaminationService, _platformRepository, _templateRepository);

        //    Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
        //    jointAction.Add("d1", Types.RouteAction.MoveRight);
        //    jointAction.Add("d2", Types.RouteAction.MoveRight);
        //    Assert.That(s1.IsGoalState(), Is.EqualTo(false));

        //    State s2 = new State(s1, jointAction);
        //    Assert.That(s2.IsGoalState(), Is.EqualTo(false));

        //    State s3 = new State(s2, jointAction);
        //    Assert.That(s3.IsGoalState(), Is.EqualTo(true));
        //}

        public Dictionary<string, Agent> CreateTwoAgentsWithPositions(int agent1X, int agent1Y, int agent2X, int agent2Y)
        {
            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", agent1X, agent1Y, 1);
            var agent2 = new Agent("d2", agent2X, agent2Y, 1);
            agents.Add("d1", agent);
            agents.Add("d2", agent2);
            return agents;
        }
    }
}