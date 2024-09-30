using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Application.ExecutionEngine.Services;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;

namespace DropletsInMotionTests
{
    public class StateTests
    {
 
        [Test]
        public void CalculateHeuristic()
        {
            byte[,] contamination = new byte[32, 20];


            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", 1, 1, 1);
            agents.Add("d1", agent);
            ICommand command = new Move("d1", 3, 3);

            State s1 = new State(new List<string>() { "d1" }, agents, contamination, new List<ICommand>() { command }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            State s2 = new State(s1, jointAction);
            Assert.AreEqual(7, s2.GetHeuristic());
            State s3 = new State(s2, jointAction);
            Assert.AreEqual(6, s3.GetHeuristic());
        }

        [Test]
        public void ExtractActionsTest()
        {
            byte[,] contamination = new byte[32, 20];


            var agents = createTwoAgentsWithPositions(1, 1, 5, 5);

            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveRight);
            State s2 = new State(s1, jointAction);
            State s3 = new State(s2, jointAction);

            Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        [Test]
        public void TestExpandedStates()
        {
            byte[,] contamination = new byte[32, 20];

            var agents = createTwoAgentsWithPositions(1,1,5,5);
            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveRight);
            List<State> expandedStates = s1.GetExpandedStates();

            Assert.AreEqual(25, expandedStates.Count());
            //State s2 = new State(s1, jointAction);
            //State s3 = new State(s2, jointAction);

            //Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        [Test]
        public void TestIsMoveApplicable()
        {
            byte[,] contamination = new byte[32, 20];


            var agents = createTwoAgentsWithPositions(1, 1, 5, 1);

            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveLeft);
            List<State> expandedStates = s1.GetExpandedStates();
            Assert.AreEqual(25, expandedStates.Count());

            State s2 = new State(s1, jointAction);
            expandedStates = s2.GetExpandedStates();
            Assert.AreEqual(9, expandedStates.Count());

            //Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        [Test]
        public void TestIsConflicting()
        {
            byte[,] contamination = new byte[32, 20];


            var agents = createTwoAgentsWithPositions(1, 1, 6, 1);

            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveLeft);
            List<State> expandedStates = s1.GetExpandedStates();
            Assert.AreEqual(25, expandedStates.Count());

            State s2 = new State(s1, jointAction);
            expandedStates = s2.GetExpandedStates();
            Assert.AreEqual(15, expandedStates.Count());

            //Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        [Test]

        public void TestIsGoalState()
        {
            byte[,] contamination = new byte[32, 20];
            var agents = createTwoAgentsWithPositions(1, 3, 5, 7);

            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveRight);
            Assert.AreEqual(false, s1.IsGoalState());

            State s2 = new State(s1, jointAction);
            Assert.AreEqual(false, s2.IsGoalState());

            State s3 = new State(s2, jointAction);
            Assert.AreEqual(true, s3.IsGoalState());

            //Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        public TemplateHandler CreateTemplateHandler()
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

            TemplateHandler templateHandler = new TemplateHandler(board);
            return templateHandler;
        }

        public Dictionary<string, Agent> createTwoAgentsWithPositions(int agent1X, int agent1Y, int agent2X, int agent2Y)
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