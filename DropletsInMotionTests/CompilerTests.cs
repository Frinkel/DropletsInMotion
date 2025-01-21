using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Infrastructure;

namespace DropletsInMotionTests
{
    public class CompilerTests : TestBase
    {

        private readonly IExecutionEngine _executionEngine;
        private readonly IUserService _userService;
        private readonly IFileService _filerService;

        public CompilerTests()
        {
            _executionEngine = ServiceProvider.GetRequiredService<IExecutionEngine>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
            _userService.ConfigurationPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/Configuration";
        }


        [SetUp]
        public void Setup()
        {
            Debugger.ExpandedStates = 0;
            Debugger.ExistingStates = 0;
            Debugger.ExploredStates = 0;
            Debugger.Nodes = new List<(int x, int y)>();
            Debugger.ElapsedTime.Clear();
            Debugger.Permutations = 0;
            Debugger.GetProcess();
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
        public async Task ManyDropletsTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";

            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/ManyDropletsTest.txt";

            await _executionEngine.Execute();

            // Assertions
            //Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            //Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(3));
            //Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(3));
        }

        [Test]
        public async Task ManyDropletsLongerRoutesTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";

            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/ManyDropletLongerRouteTest.txt";

            await _executionEngine.Execute();

            // Assertions
            //Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
            //Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(3));
            //Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(3));
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

        [Test]
        public async Task VariablesTest1()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestVariables1.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(5));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(5));
        }

        [Test]
        public async Task VariablesTest2()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestVariables2.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(5));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(5));
            Assert.That(_executionEngine.Agents["d2"].PositionX, Is.EqualTo(14));
            Assert.That(_executionEngine.Agents["d2"].PositionY, Is.EqualTo(14));
        }

        [Test]
        public async Task WhileLoopTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestWhileLoop.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(15));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }


        [Test]
        public async Task IfTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestIf.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(10));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task IfElseTest1()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestIfElse1.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(10));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task IfElseTest2()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestIfElse2.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(1));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(6));
        }

        [Test]
        public async Task WhileIfElseTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestWhileLoopIfElse.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(5));
            Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(10));
        }

        [Test]
        public async Task WasteTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestWaste.txt";

            await _executionEngine.Execute();

            // Assertions
            Assert.IsFalse(_executionEngine.Agents.ContainsKey("d1"));
        }

        [Test]
        public async Task DropletDeclarationIncorrectFailTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestDropletDeclarationFailure.txt";

            Assert.CatchAsync(async () => await _executionEngine.Execute());
        }

        [Test]
        public async Task CannotMoveDropletFailTest()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestCannotMoveDropletsFail.txt";

            Assert.CatchAsync(async () => await _executionEngine.Execute());
        }

        [Test]
        public async Task ParallelTest1()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestParallelRun1.txt";

            await _executionEngine.Execute();

            Assert.That(_executionEngine.Time, Is.EqualTo(2.0d));
        }

        [Test]
        public async Task A1DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/1DropletCrissCross.txt";



            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }

        [Test]
        public async Task A2DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/2DropletCrissCross.txt";

            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }
        [Test]
        public async Task A3DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/3DropletCrissCross.txt";

            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }

        [Test]
        public async Task A4DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/4DropletCrissCross.txt";

            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");


        }
        [Test]
        public async Task A5DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/5DropletCrissCross.txt";

            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }
        [Test]
        public async Task A6DropletCrissCross()
        {
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/6DropletCrissCross.txt";

            await _executionEngine.Execute();
            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }
        [Test]
        public async Task A7DropletCrissCross()
        {
            
            _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/7DropletCrissCross.txt";

            await _executionEngine.Execute();

            Console.WriteLine($"Explored {Debugger.ExploredStates} - Existing {Debugger.ExistingStates} - Expanded {Debugger.ExpandedStates} - Permutations {Debugger.Permutations}");

        }

        [Test]
        public async Task A0SoakTask()
        {
            //_userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
            //_userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/program.txt";

            //await _executionEngine.Execute();

        }

    }
}