using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using static MicrofluidicsParser;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Presentation.Language;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Services;

namespace DropletsInMotionTests
{
    public class CompilerTests : TestBase
    {

        private readonly IExecutionEngine _executionEngine;


        private string projectDirectory;
        private string platformPath;

        public CompilerTests()
        {
            _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
        }

        [SetUp]
        public void Setup()
        {
            string workingDirectory = Environment.CurrentDirectory;
            projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
            platformPath = Path.Combine(projectDirectory, "Assets", "platform.json");
            //communicationEngine = new CommunicationService(true);
            _executionEngine.Agents.Clear();
        }

        [Test]
        public async Task MoveOneDropletTest()
        {
            string contents = "Droplet(d1, 15, 15, 0.2);\r\nMove(d1, 3, 3);\r\n";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(3));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(3));
        }

        [Test]
        public async Task MergeDropletsTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\n\r\nMerge(d1, d2, d6, 10, 10);\r\nMove(d6,11,11);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d6"].PositionX, Is.EqualTo(11));
            Assert.That(_executionEngine.Agents["d6"].PositionY, Is.EqualTo(11));
        }

        [Test]
        public async Task MergeDropletsDropletNameReusedTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\n\r\nMerge(d1, d2, d1, 10, 10);\r\nMove(d1,11,11);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(11));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(11));
        }

        [Test]
        public async Task SplitByVolumeTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d2, d3, 2, 6, 9, 6, 0.6);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionX, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionY, Is.EqualTo(6));
            Assert.That(_executionEngine.Agents["d3"].PositionX, Is.EqualTo(9));
            Assert.That(_executionEngine.Agents["d3"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task MixTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nMix(d1, 10, 10, 3, 3, 20);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(10));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(10));
        }

        [Test]
        public async Task SplitByVolumeNameReusedTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d2, d1, 2, 6, 9, 6, 0.6);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionX, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionY, Is.EqualTo(6));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(9));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task SplitByRatioTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByRatio(d1, d2, d3, 2, 6, 9, 6, 0.5);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionX, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d2"].PositionY, Is.EqualTo(6));
            Assert.That(_executionEngine.Agents["d3"].PositionX, Is.EqualTo(9));
            Assert.That(_executionEngine.Agents["d3"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task SplitByRatioNameReusedTest()
        {
            string contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d1, d2, 2, 6, 9, 6, 0.5);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(2));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
            Assert.That(_executionEngine.Agents["d2"].PositionX, Is.EqualTo(9));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task BigProgramTest()
        {
            string contents =
                "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\nDroplet(d3, 17, 6, 1.2);\r\nMerge(d1, d2, d4, 8, 6);\r\nMove(d4, 8, 11);\r\nSplitByVolume(d4, d1, d2, 4, 11, 12, 11, 1.2);\r\nMove(d1, 4, 13);\r\nMove(d2, 12, 14);\r\nSplitByRatio(d1, d1, d4, 2, 13, 6, 13, 0.5);\r\nSplitByRatio(d2, d2, d5, 10, 14, 14, 14, 0.5);\r\nMix(d3, 20, 6, 4, 4, 1);\r\nMerge(d5, d3, d3, 16, 14);\r\nStore(d3, 16, 14, 2000);\r\nMove(d3, 18, 18);";
            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            await _executionEngine.Execute(listener.Commands, listener.Droplets, platformPath);

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(4));
            Assert.GreaterOrEqual(_executionEngine.Time, 2001);
        }


        public ProgramContext ParseProgram(string programContents)
        {
            var inputStream = new AntlrInputStream(programContents);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);
            var tree = parser.program();
            return tree;
        }

    }
}