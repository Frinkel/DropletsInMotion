﻿using System.Runtime.Intrinsics.X86;
using Antlr4.Runtime;
using DropletsInMotion;
using DropletsInMotion.Compilers;
using DropletsInMotion.Domain;
using System.Xml.Linq;
using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Controllers;
using NUnit.Framework;
using DropletsInMotion.Compilers.Services;
using DropletsInMotion.Routers;
using DropletsInMotion.Routers.Models;

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
            Assert.AreEqual(4, s2.GetHeuristic());
            State s3 = new State(s2, jointAction);
            Assert.AreEqual(4, s3.GetHeuristic());
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
            Assert.AreEqual(24, expandedStates.Count());

            //Assert.AreEqual(12, s3.ExtractActions(0).Count);
        }

        [Test]

        public void TestIsGoalState()
        {
            byte[,] contamination = new byte[32, 20];
            var agents = createTwoAgentsWithPositions(1, 1, 5, 7);

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

        [Test]
        public void TestAStarSearchSimpleRoute()
        {
            byte[,] contamination = new byte[32, 20];
            var agents = createTwoAgentsWithPositions(1, 1, 5, 7);

            ICommand command = new Move("d1", 31, 18);
            ICommand command2 = new Move("d2", 17, 17);
            var commands = new List<ICommand>() { command, command2 };

            var routableAgents = new List<string>() { "d1", "d2" };
            State s0 = new State(routableAgents, agents, contamination, commands, CreateTemplateHandler());

            Frontier frontier = new Frontier();
            AstarRouter astarRouter = new AstarRouter();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var res = astarRouter.Search(s0, frontier, 0);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

            Assert.AreEqual(res.Item3.Count, 207);
        }

        [Test]
        public void TestAStarSearchAroundEachother()
        {
            byte[,] contamination = new byte[32, 20];
            var agents = createTwoAgentsWithPositions(5, 5, 12, 5);

            ICommand command = new Move("d1", 20, 5);
            ICommand command2 = new Move("d2", 1, 5);
            var commands = new List<ICommand>() { command, command2 };

            var routableAgents = new List<string>() { "d1", "d2" };
            State s0 = new State(routableAgents, agents, contamination, commands, CreateTemplateHandler());

            Frontier frontier = new Frontier();
            AstarRouter astarRouter = new AstarRouter();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var res = astarRouter.Search(s0, frontier, 0);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs.ToString());

            Assert.AreEqual(res.Item3.Count, 12);


            //Assert.AreEqual(true, false);
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