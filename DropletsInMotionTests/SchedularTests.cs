using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class SchedularTests : TestBase
    {
        private readonly IContaminationService _contaminationService;
        private readonly ISchedulerService _schedulerService;

        public SchedularTests()
        {
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _schedulerService = ServiceProvider.GetRequiredService<ISchedulerService>();
        }

        [SetUp]
        public void Setup()
        {
            Agent.ResetSubstanceId();
        }

        [Test]
        public void MergePosition()
        {
            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 8, 0, 1);
            var a2 = new Agent("a2", 0, 0, 1);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];
            _contaminationService.UpdateContaminationArea(contaminationMap, 255, 2, 0, 4, 0);
            _contaminationService.PrintContaminationState(contaminationMap);
            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap);

            Assert.That(6, Is.EqualTo(optimalPosition.Value.Item1.optimalX));
            Assert.That(1, Is.EqualTo(optimalPosition.Value.Item1.optimalY));
            Assert.That(4, Is.EqualTo(optimalPosition.Value.Item2.optimalX));
            Assert.That(1, Is.EqualTo(optimalPosition.Value.Item2.optimalY));

        }

        [Test]
        public void MergePositionCloseToEachother()
        {
            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 6, 0, 1);
            var a2 = new Agent("a2", 4, 0, 1);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap);

            Assert.That(6, Is.EqualTo(optimalPosition.Value.Item1.optimalX));
            Assert.That(0, Is.EqualTo(optimalPosition.Value.Item1.optimalY));
            Assert.That(4, Is.EqualTo(optimalPosition.Value.Item2.optimalX));
            Assert.That(0, Is.EqualTo(optimalPosition.Value.Item2.optimalY));

        }


        [Test]
        public void SplitPosition()
        {
            IDropletCommand splitCommand = new SplitByVolume("a1", "a2", "a3", 0, 0, 8, 0, 0.5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = new Agent("a1", 5, 5, 1);
            agents.Add("a1", a1);

            var board = CreateBoard();
            var contaminationMap = new byte[board.Length, board[0].Length];

            var optimalPosition = _schedulerService.ScheduleCommand(splitCommand, agents, contaminationMap);

            Assert.That(4, Is.EqualTo(optimalPosition.Value.Item1.optimalX));
            Assert.That(0, Is.EqualTo(optimalPosition.Value.Item1.optimalY));
            Assert.That(6, Is.EqualTo(optimalPosition.Value.Item2.optimalX));
            Assert.That(0, Is.EqualTo(optimalPosition.Value.Item2.optimalY));

        }


        //[Test]
        //public void SplitPosition()
        //{
        //    IDropletCommand splitCommand = new SplitByVolume("d1", "d2", "d3", 1, 0, 18, 0, 2);
        //    var commands = new List<IDropletCommand>() { splitCommand };

        //    Dictionary<string, Droplet> droplets = new Dictionary<string, Droplet>();
        //    var d1 = new Droplet("d1", 2, 7, 1);

        //    droplets.Add("d1", d1);

        //    Scheduler scheduler = new Scheduler();
        //    var splitPositions = scheduler.ScheduleCommand(splitCommand, droplets);

        //    Console.WriteLine(splitPositions);

        //    Assert.AreEqual(splitPositions.Item1.d1OptimalX, 1);
        //    Assert.AreEqual(splitPositions.Item1.d1OptimalY, 7);

        //    Assert.AreEqual(splitPositions.Item2.d2OptimalX, 3);
        //    Assert.AreEqual(splitPositions.Item2.d2OptimalY, 7);

        //}

        public Electrode[][] CreateBoard()
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
            return board;
        }

    }
}