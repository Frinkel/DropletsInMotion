using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Communication;
using static MicrofluidicsParser;
using DropletsInMotion.Application.ExecutionEngine;
using DropletsInMotion.Presentation.Language;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Services;
using NUnit.Framework;

namespace DropletsInMotionTests
{
    public class CompilerTests : TestBase
    {

        private readonly IExecutionEngine _executionEngine;
        private readonly IUserService _userService;
        private readonly IFileService _filerService;

        private string projectDirectory;
        private string platformPath;

        public CompilerTests()
        {
            _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
        }

        [Test]
        public async Task MoveOneDropletTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMoveDroplet.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(3));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(3));
        }

        [Test]
        public async Task MergeDropletsTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMergeDroplets.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d6"].PositionX, Is.EqualTo(11));
            Assert.That(_executionEngine.Agents["d6"].PositionY, Is.EqualTo(11));
        }

        [Test]
        public async Task MergeDropletsDropletNameReusedTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMergeDropletsReusedNames.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(11));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(11));
        }

        [Test]
        public async Task SplitByVolumeTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestSplitByVolume.txt";

            await _executionEngine.Execute();

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
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMix.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(10));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(10));
        }

        [Test]
        public async Task SplitByVolumeNameReusedTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestSplitByVolumeNamesReused.txt";

            await _executionEngine.Execute();

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
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestSplitByRatio.txt";

            await _executionEngine.Execute();

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

            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestSplitByRatioNamesReused.txt";

            await _executionEngine.Execute();

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
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestBigProgram.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents.Count, Is.EqualTo(4));
            Assert.GreaterOrEqual(_executionEngine.Time, 2001);
        }
    }
}