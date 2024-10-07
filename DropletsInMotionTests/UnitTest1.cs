//using Antlr4.Runtime;
//using DropletsInMotion.Application.ExecutionEngine.Models;
//using DropletsInMotion.Presentation.Services;
//using DropletsInMotion.Infrastructure.Models.Domain;
//using DropletsInMotion.Application.Services;

//namespace DropletsInMotionTests
//{
//    public class Tests
//    {
//        [SetUp]
//        public void Setup()
//        {
//        }

//        [Test]
//        public void ParserTest1()
//        {
//            String contents = "Droplet(d1, 15, 15, 0.2);\r\nMove(d1, 3, 3);\r\n";
//            var inputStream = new AntlrInputStream(contents);
//            var lexer = new MicrofluidicsLexer(inputStream);
//            var commonTokenStream = new CommonTokenStream(lexer);
//            var parser = new MicrofluidicsParser(commonTokenStream);

//            // Get the root of the parse tree (starting with 'program')
//            var tree = parser.program();
//            Assert.AreEqual(tree.ToStringTree(parser), "(program (command (dropletDeclaration Droplet ( d1 , 15 , 15 , 0.2 ))) ; (command (moveDroplet Move ( d1 , 3 , 3 ))) ; <EOF>)");
//        }

//        [Test]
//        public void TemplateTester()
//        {
//            var board = new Electrode[32][];
//            for (int i = 0; i < 32; i++)
//            {
//                board[i] = new Electrode[20];
//                for (int j = 0; j < 20; j++)
//                {
//                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
//                }
//            }

//            TemplateService templateHandler = new TemplateService();
//            templateHandler.Initialize(board);
//            List<(string, List<BoardAction>)> templates = templateHandler.Templates;
//            Console.WriteLine(templates);
//            Assert.AreEqual(templates.Count, 9);
//            Assert.AreEqual(templates[0].Item1, "mergeHorizontal");

//        }

//        [Test]
//        public void PlatformServiceTester()
//        {
//            string workingDirectory = Environment.CurrentDirectory;
//            string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
//            string platformPath = Path.Combine(projectDirectory, "Assets", "platform.json");
//            PlatformService platformService = new PlatformService(platformPath);
//            Electrode[][] board = platformService.Board;
//            Assert.AreEqual(board.Length, 32);
//            Assert.AreEqual(board[0].Length, 20);
//            Assert.AreEqual(board[0][0].Id, 1);
//            Assert.AreEqual(board[31][19].Id, 640);
//            Assert.AreEqual(board[23][10].Id, 344);
//        }
//        //[Test]
//        //public void CompilerTest()
//        //{
//        //    List<Droplet> droplets = new List<Droplet>();
//        //    droplets.Add(new Droplet("d1", 1, 1, 1));
//        //    List<Move> moves = new List<Move>();
//        //    moves.Add(new Move("d1",2,2));
//        //    moves.Add(new Move("d1", 3, 3));
//        //    string workingDirectory = Environment.CurrentDirectory;
//        //    string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
//        //    string platformPath = Path.Combine(projectDirectory, "TestAssets", "platform.json");
//        //    Compiler compiler = new Compiler(droplets, moves, null, platformPath);
//        //    List<BoardAction> actions = compiler.Execute();

//        //    Assert.AreEqual(8, actions.Count());
//        //}

//    }
//}