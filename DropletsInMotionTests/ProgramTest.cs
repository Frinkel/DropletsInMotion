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
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using DropletsInMotion.Language;
using static MicrofluidicsParser;

namespace DropletsInMotionTests
{
    public class CompilerTests
    {
        private string projectDirectory;
        private string platformPath;
        private CommunicationEngine communicationEngine;

        [SetUp]
        public void Setup()
        {
            string workingDirectory = Environment.CurrentDirectory;
            projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
            platformPath = Path.Combine(projectDirectory, "Assets", "platform.json");
            communicationEngine = new CommunicationEngine(true);
        }

        [Test]
        public async Task MoveOneDropletTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 15, 15, 0.2);\r\nMove(d1, 3, 3);\r\n";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(1, compiler.Droplets.Count);
            Assert.AreEqual(3, compiler.Droplets["d1"].PositionX);
            Assert.AreEqual(3, compiler.Droplets["d1"].PositionY);
        }

        [Test]
        public async Task MergeDropletsTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\n\r\nMerge(d1, d2, d6, 10, 10);\r\nMove(d6,11,11);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(1, compiler.Droplets.Count);
            Assert.AreEqual(11, compiler.Droplets["d6"].PositionX);
            Assert.AreEqual(11, compiler.Droplets["d6"].PositionY);
        }

        [Test]
        public async Task MergeDropletsDropletNameReusedTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\n\r\nMerge(d1, d2, d1, 10, 10);\r\nMove(d1,11,11);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(1, compiler.Droplets.Count);
            Assert.AreEqual(11, compiler.Droplets["d1"].PositionX);
            Assert.AreEqual(11, compiler.Droplets["d1"].PositionY);
        }

        [Test]
        public async Task SplitByVolumeTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d2, d3, 2, 6, 9, 6, 0.6);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(2, compiler.Droplets.Count);
            Assert.AreEqual(2, compiler.Droplets["d2"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d2"].PositionY);
            Assert.AreEqual(9, compiler.Droplets["d3"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d3"].PositionY);
        }

        [Test]
        public async Task MixTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nMix(d1, 10, 10, 3, 3, 20);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(1, compiler.Droplets.Count);
            Assert.AreEqual(10, compiler.Droplets["d1"].PositionX);
            Assert.AreEqual(10, compiler.Droplets["d1"].PositionY);
        }

        [Test]
        public async Task SplitByVolumeNameReusedTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d2, d1, 2, 6, 9, 6, 0.6);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(2, compiler.Droplets.Count);
            Assert.AreEqual(2, compiler.Droplets["d2"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d2"].PositionY);
            Assert.AreEqual(9, compiler.Droplets["d1"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d1"].PositionY);
        }

        [Test]
        public async Task SplitByRatioTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByRatio(d1, d2, d3, 2, 6, 9, 6, 0.5);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(2, compiler.Droplets.Count);
            Assert.AreEqual(2, compiler.Droplets["d2"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d2"].PositionY);
            Assert.AreEqual(9, compiler.Droplets["d3"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d3"].PositionY);
        }

        [Test]
        public async Task SplitByRatioNameReusedTest()
        {
            // Test-specific content
            String contents = "Droplet(d1, 5, 6, 1.2);\r\nSplitByVolume(d1, d1, d2, 2, 6, 9, 6, 0.5);";

            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(2, compiler.Droplets.Count);
            Assert.AreEqual(2, compiler.Droplets["d1"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d1"].PositionY);
            Assert.AreEqual(9, compiler.Droplets["d2"].PositionX);
            Assert.AreEqual(6, compiler.Droplets["d1"].PositionY);
        }

        [Test]
        public async Task BigProgramTest()
        {
            // Test-specific content
            String contents =
                "Droplet(d1, 5, 6, 1.2);\r\nDroplet(d2, 11, 6, 1.2);\r\nDroplet(d3, 17, 6, 1.2);\r\nMerge(d1, d2, d4, 8, 6);\r\nMove(d4, 8, 11);\r\nSplitByVolume(d4, d1, d2, 4, 11, 12, 11, 1.2);\r\nMove(d1, 4, 13);\r\nMove(d2, 12, 14);\r\nSplitByRatio(d1, d1, d4, 2, 13, 6, 13, 0.5);\r\nSplitByRatio(d2, d2, d5, 10, 14, 14, 14, 0.5);\r\nMix(d3, 20, 6, 4, 4, 1);\r\nMerge(d5, d3, d3, 16, 14);\r\nStore(d3, 16, 14, 2000);\r\nMove(d3, 18, 18);";
            var listener = new MicrofluidicsCustomListener();
            var tree = ParseProgram(contents);
            ParseTreeWalker.Default.Walk(listener, tree);

            // Initialize the compiler with listener data and platform path
            Compiler compiler = new Compiler(listener.Commands, listener.Droplets, null, platformPath);
            await compiler.Compile();

            // Assertions
            Assert.AreEqual(4, compiler.Droplets.Count);
            Assert.GreaterOrEqual(compiler.time, 2001);
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