using Antlr4.Runtime;
using DropletsInMotion;

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
    }
}