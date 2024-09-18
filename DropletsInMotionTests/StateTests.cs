using System.Runtime.Intrinsics.X86;
using Antlr4.Runtime;
using DropletsInMotion;
using DropletsInMotion.Compilers;
using DropletsInMotion.Domain;
using System.Xml.Linq;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Controllers;
using NUnit.Framework;
using DropletsInMotion.Compilers.Services;
using DropletsInMotion.Routers.Models;

namespace DropletsInMotionTests
{
    public class StateTests
    {
 
        [Test]
        public void calculateHeuristic()
        {
            byte[,] contamination = new byte[20, 32];


            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", 1, 1, 1);
            agents.Add("d1", agent);
            ICommand command = new Move("d1", 3, 3);

            State s1 = new State(new List<string>() { "d1" }, agents, contamination, new List<ICommand>() { command }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            State s2 = new State(s1, jointAction);
            Assert.AreEqual(4, s2.GetHeuristic());
            State s3 = new State(s2, jointAction);
            Assert.AreEqual(4, s3.GetHeuristic());
        }

        [Test]
        public void extractActionsTest()
        {
            byte[,] contamination = new byte[20, 32];


            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", 1, 1, 1);
            var agent2 = new Agent("d2", 5, 5, 1);
            agents.Add("d1", agent);
            agents.Add("d2", agent2);
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
        public void testExpandedStates()
        {
            byte[,] contamination = new byte[20, 32];


            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", 1, 1, 1);
            var agent2 = new Agent("d2", 5, 5, 1);
            agents.Add("d1", agent);
            agents.Add("d2", agent2);
            ICommand command = new Move("d1", 3, 3);
            ICommand command2 = new Move("d2", 7, 7);

            State s1 = new State(new List<string>() { "d1", "d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveRight);
            List<State> expandedStates = s1.GetExpandedStates();

            Assert.AreEqual(12, expandedStates.Count());
            //State s2 = new State(s1, jointAction);
            //State s3 = new State(s2, jointAction);

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
    }
}