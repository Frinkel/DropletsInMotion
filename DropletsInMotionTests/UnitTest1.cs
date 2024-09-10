using Antlr4.Runtime;
using DropletsInMotion;
using DropletsInMotion.Compilers;
using DropletsInMotion.Domain;
using System.Xml.Linq;
using DropletsInMotion.Compilers.Models;

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
            Assert.AreEqual(tree.ToStringTree(parser), "(program (statement (dropletDeclaration Droplet ( d1 , 15 , 15 , 0.2 ))) ; (statement (moveDroplet Move ( d1 , 3 , 3 ))) ; <EOF>)");
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
        //    List<BoardActionDto> actions = compiler.Compile();

        //    Assert.AreEqual(8, actions.Count());
        //}
    }
}