using Antlr4.Runtime;
using DropletsInMotion.Presentation.Services;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation;
using DropletsInMotion.Translation;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class TypeCheckerTests : TestBase
    {
        private readonly ITranslator _translator;
        private readonly IUserService _userService;
        private readonly IFileService _filerService;

        private string projectDirectory;
        private string platformPath;

        public TypeCheckerTests()
        {
            _translator = ServiceProvider.GetRequiredService<ITranslator>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
            _userService.ConfigurationPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/Configuration";

        }

        [Test]
        public void ParseBoardTest()
        {

            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMoveDroplet.txt";

            _translator.Translate();

            Assert.That(_translator.Board.Length, Is.EqualTo(32));
            Assert.That(_translator.Board[0].Length, Is.EqualTo(20));
        }

        [Test]
        public void ParserProgramNoNestedGraphs1()
        {

            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMoveDroplet.txt";

            _translator.Translate();

            Assert.That(_translator.DependencyGraph.GetAllNodes().Count, Is.EqualTo(2));
            Assert.That(_translator.Commands.Count, Is.EqualTo(2));
        }

        [Test]
        public void ParserProgramNoNestedGraphs2()
        {

            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMergeDroplets.txt";

            _translator.Translate();

            Assert.That(_translator.DependencyGraph.GetAllNodes().Count, Is.EqualTo(4));
            Assert.That(_translator.Commands.Count, Is.EqualTo(4));
        }

        [Test]
        public void ParserProgramNoNestedGraphs3()
        {

            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestBigProgram.txt";

            _translator.Translate();

            Assert.That(_translator.DependencyGraph.GetAllNodes().Count, Is.EqualTo(14));
            Assert.That(_translator.Commands.Count, Is.EqualTo(14));
        }

        [Test]
        public void ParserProgramNestedGraphs1()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestWhileLoop.txt";

            _translator.Translate();

            Assert.That(_translator.DependencyGraph.GetAllNodes().Count, Is.EqualTo(3));
            Assert.IsTrue(_translator.DependencyGraph.GetAllNodes().Exists(n => n is DependencyNodeWhile));
            Assert.That(_translator.Commands.Count, Is.EqualTo(3));
        }

        [Test]
        public void ParserProgramPcr()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestPcrProgram.txt";

            _translator.Translate();

            Assert.That(_translator.DependencyGraph.GetAllNodes().Count, Is.EqualTo(20));
        }

        [Test]
        public void CannotParseProgramTest1()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse1.txt";
            Assert.Catch(() => _translator.Translate());
        }

        [Test]
        public void CannotParseProgramTest2()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse2.txt";
            Assert.Catch(() => _translator.Translate());
        }

        [Test]
        public void CannotParseProgramTest3()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse3.txt";
            Assert.Catch(() => _translator.Translate());
        }

        [Test]
        public void CannotParseProgramTest4()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse4.txt";
            Assert.Catch(() => _translator.Translate());
        }

        [Test]
        public void CannotParseProgramTest5()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse5.txt";
            Assert.Catch(() => _translator.Translate());
        }

        [Test]
        public void CannotParseProgramTest6()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestProgramFailParse6.txt";
            Assert.Catch(() => _translator.Translate());
        }


    }
}