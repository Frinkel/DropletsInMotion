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
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ParserTest1()
        {
            String contents = "Droplet(d1, 15, 15, 0.2);\r\nMove(d1, 3, 3);\r\n";
            var inputStream = new AntlrInputStream(contents);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);

            // Get the root of the parse tree (starting with 'program')
            var tree = parser.program();
            Assert.AreEqual(tree.ToStringTree(parser), "(program (command (dropletDeclaration Droplet ( d1 , 15 , 15 , 0.2 ))) ; (command (moveDroplet Move ( d1 , 3 , 3 ))) ; <EOF>)");
        }

        [Test]
        public void templateTester()
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
            List<(string, List<BoardAction>)> templates = templateHandler.templates;
            Console.WriteLine(templates);
            Assert.AreEqual(templates.Count, 9);
            Assert.AreEqual(templates[0].Item1, "mergeHorizontal");

        }

        [Test]
        public void platformServiceTester()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
            string platformPath = Path.Combine(projectDirectory, "Assets", "platform.json");
            PlatformService platformService = new PlatformService(platformPath);
            Electrode[][] board = platformService.Board;
            Assert.AreEqual(board.Length, 32);
            Assert.AreEqual(board[0].Length, 20);
            Assert.AreEqual(board[0][0].Id, 1);
            Assert.AreEqual(board[31][19].Id, 640);
            Assert.AreEqual(board[23][10].Id, 344);
        }
        //[Test]
        //public void CompilerTest()
        //{
        //    List<Droplet> droplets = new List<Droplet>();
        //    droplets.Add(new Droplet("d1", 1, 1, 1));
        //    List<Move> moves = new List<Move>();
        //    moves.Add(new Move("d1",2,2));
        //    moves.Add(new Move("d1", 3, 3));
        //    string workingDirectory = Environment.CurrentDirectory;
        //    string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
        //    string platformPath = Path.Combine(projectDirectory, "TestAssets", "platform.json");
        //    Compiler compiler = new Compiler(droplets, moves, null, platformPath);
        //    List<BoardAction> actions = compiler.Compile();

        //    Assert.AreEqual(8, actions.Count());
        //}


        [Test]
        public void calculateHeuristic()
        {
            byte[,] contamination = new byte[20, 32];


            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var agent = new Agent("d1", 1, 1, 1);
            agents.Add("d1", agent);
            ICommand command = new Move("d1", 3, 3);

            State s1 = new State(new List<string>(){"d1"}, agents, contamination, new List<ICommand>(){command}, CreateTemplateHandler());
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

            State s1 = new State(new List<string>() { "d1","d2" }, agents, contamination, new List<ICommand>() { command, command2 }, CreateTemplateHandler());
            Dictionary<string, Types.RouteAction> jointAction = new Dictionary<string, Types.RouteAction>();
            jointAction.Add("d1", Types.RouteAction.MoveRight);
            jointAction.Add("d2", Types.RouteAction.MoveRight);
            State s2 = new State(s1, jointAction);
            State s3 = new State(s2, jointAction);

            Assert.AreEqual(12, s3.ExtractActions(0).Count);
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