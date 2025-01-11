using DropletsInMotion.Application.Factories;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class SchedularTests : TestBase
    {
        private readonly IContaminationService _contaminationService;
        private readonly ISchedulerService _schedulerService;
        private readonly IAgentFactory _agentFactory;
        private readonly IContaminationRepository _contaminationRepository;
        private readonly ITemplateRepository _templateRepository;

        public SchedularTests()
        {
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _schedulerService = ServiceProvider.GetRequiredService<ISchedulerService>();
            _agentFactory = ServiceProvider.GetRequiredService<IAgentFactory>();
            _contaminationRepository = ServiceProvider.GetRequiredService<IContaminationRepository>();
            _templateRepository = ServiceProvider.GetRequiredService<ITemplateRepository>();

        }

        [SetUp]
        public void Setup()
        {
            Agent.ResetSubstanceId();
        }

        [Test]
        public void MergePosition()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 8, 0, 706, 1);
            var a2 = _agentFactory.CreateAgent("a2", 0, 0, 706, 1);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            _contaminationService.UpdateContaminationArea(contaminationMap, 2, 2, 0, 4, 0);

            var a1Substance = _contaminationService.GetSubstanceId("A");
            var a2Substance = _contaminationService.GetSubstanceId("b");

            List<ITemplate> eligibleMergeTemplates = _templateRepository?
            .MergeTemplates?
            .FindAll(t => t.MinSize <= a1.Volume + a2.Volume && a1.Volume + a2.Volume < t.MaxSize)
            ?.Cast<ITemplate>()
            .ToList() ?? new List<ITemplate>();

            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap, eligibleMergeTemplates);

            Assert.That(7, Is.EqualTo(optimalPosition.X1));
            Assert.That(1, Is.EqualTo(optimalPosition.Y2));
            Assert.That(4, Is.EqualTo(optimalPosition.X2));
            Assert.That(1, Is.EqualTo(optimalPosition.Y2));

        }

        [Test]
        public void MergePositionCloseToEachother()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 6, 0, 380, 1);
            var a2 = _agentFactory.CreateAgent("a2", 4, 0, 380, 1);
            agents.Add("a1", a1);
            agents.Add("a2", a2);

            List<ITemplate> eligibleMergeTemplates = _templateRepository?
                .MergeTemplates?
                .FindAll(t => t.MinSize <= a1.Volume + a2.Volume && a1.Volume + a2.Volume < t.MaxSize)
                ?.Cast<ITemplate>()
                .ToList() ?? new List<ITemplate>();

            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap, eligibleMergeTemplates);

            Assert.That(6, Is.EqualTo(optimalPosition.X1));
            Assert.That(0, Is.EqualTo(optimalPosition.Y2));
            Assert.That(4, Is.EqualTo(optimalPosition.X2));
            Assert.That(0, Is.EqualTo(optimalPosition.Y2));

            //var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap);

            //Assert.That(6, Is.EqualTo(optimalPosition.Value.Item1.optimalX));
            //Assert.That(0, Is.EqualTo(optimalPosition.Value.Item1.optimalY));
            //Assert.That(4, Is.EqualTo(optimalPosition.Value.Item2.optimalX));
            //Assert.That(0, Is.EqualTo(optimalPosition.Value.Item2.optimalY));

        }

        [Test]
        public void MergePosition2()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 0, 0, 706, 1);
            var a2 = _agentFactory.CreateAgent("a2", 31, 19, 706, 1);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var a1Substance = _contaminationService.GetSubstanceId("A");
            var a2Substance = _contaminationService.GetSubstanceId("b");

            List<ITemplate> eligibleMergeTemplates = _templateRepository?
                .MergeTemplates?
                .FindAll(t => t.MinSize <= a1.Volume + a2.Volume && a1.Volume + a2.Volume < t.MaxSize)
                ?.Cast<ITemplate>()
                .ToList() ?? new List<ITemplate>();

            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap, eligibleMergeTemplates);

            Console.WriteLine(optimalPosition);

            Assert.That(4, Is.EqualTo(optimalPosition.X1));
            Assert.That(5, Is.EqualTo(optimalPosition.Y1));
            Assert.That(7, Is.EqualTo(optimalPosition.X2));
            Assert.That(5, Is.EqualTo(optimalPosition.Y2));

        }

        [Test]
        public void MergePosition3()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            IDropletCommand mergeCommand = new Merge("a1", "a2", "a3", 5, 5);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 8, 0, 706, 1);
            var a2 = _agentFactory.CreateAgent("a2", 30, 10, 706, 1);

            agents.Add("a1", a1);
            agents.Add("a2", a2);

            var a1Substance = _contaminationService.GetSubstanceId("A");
            var a2Substance = _contaminationService.GetSubstanceId("b");

            List<ITemplate> eligibleMergeTemplates = _templateRepository?
                .MergeTemplates?
                .FindAll(t => t.MinSize <= a1.Volume + a2.Volume && a1.Volume + a2.Volume < t.MaxSize)
                ?.Cast<ITemplate>()
                .ToList() ?? new List<ITemplate>();

            var optimalPosition = _schedulerService.ScheduleCommand(mergeCommand, agents, contaminationMap, eligibleMergeTemplates);

            Console.WriteLine(optimalPosition);

            Assert.That(8, Is.EqualTo(optimalPosition.X1));
            Assert.That(3, Is.EqualTo(optimalPosition.Y1));
            Assert.That(8, Is.EqualTo(optimalPosition.X2));
            Assert.That(6, Is.EqualTo(optimalPosition.Y2));

        }


        [Test]
        public void SplitPosition()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            SplitByVolume splitByVolumeCommand = new SplitByVolume("a1", "a2", "a3", 0, 0, 8, 0, 400);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 5, 5, 800, 1);

            agents.Add("a1", a1);


            Agent splitAgent = a1;
            double agentVolume = splitAgent.Volume;
            double ratio = splitByVolumeCommand.Volume / splitAgent.Volume;

            List<ITemplate> eligibleSplitTemplates = new List<ITemplate>();

            foreach (var template in _templateRepository.SplitTemplates)
            {
                if ((template.MinSize <= agentVolume && agentVolume < template.MaxSize &&
                      Math.Abs(template.Ratio - ratio) < 0.1))
                {

                    if (Math.Abs(template.RatioRelation.First().Value - ratio) < 0.1) // TODO: Should this tolerance be user defined
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                    else
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                }
            }

            var optimalPosition = _schedulerService.ScheduleCommand(splitByVolumeCommand, agents, contaminationMap, eligibleSplitTemplates);
            Console.WriteLine(optimalPosition);
            Assert.That(4, Is.EqualTo(optimalPosition.X1));
            Assert.That(0, Is.EqualTo(optimalPosition.Y1));
            Assert.That(6, Is.EqualTo(optimalPosition.X2));
            Assert.That(0, Is.EqualTo(optimalPosition.Y2));
        }

        [Test]
        public void SplitPositionFarApart()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            SplitByVolume splitByVolumeCommand = new SplitByVolume("a1", "a2", "a3", 0, 0, 31, 19, 400);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 14, 9, 800, 1);

            agents.Add("a1", a1);


            Agent splitAgent = a1;
            double agentVolume = splitAgent.Volume;
            double ratio = splitByVolumeCommand.Volume / splitAgent.Volume;

            List<ITemplate> eligibleSplitTemplates = new List<ITemplate>();

            foreach (var template in _templateRepository.SplitTemplates)
            {
                if ((template.MinSize <= agentVolume && agentVolume < template.MaxSize &&
                      Math.Abs(template.Ratio - ratio) < 0.1))
                {

                    if (Math.Abs(template.RatioRelation.First().Value - ratio) < 0.1) // TODO: Should this tolerance be user defined
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                    else
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                }
            }

            var optimalPosition = _schedulerService.ScheduleCommand(splitByVolumeCommand, agents, contaminationMap, eligibleSplitTemplates);
            Console.WriteLine(optimalPosition);
            Assert.That(13, Is.EqualTo(optimalPosition.X1));
            Assert.That(9, Is.EqualTo(optimalPosition.Y1));
            Assert.That(15, Is.EqualTo(optimalPosition.X2));
            Assert.That(9, Is.EqualTo(optimalPosition.Y2));
        }

        [Test]
        public void SplitPositionBlockingArea()
        {
            var board = CreateBoard();
            var contaminationMap = _contaminationService.CreateContaminationMap(board.Length, board[0].Length);

            foreach (var bl in _contaminationRepository.ContaminationTable)
            {
                foreach (var b in bl)
                {
                    Console.Write(b);
                }

                Console.WriteLine();
            }

            SplitByVolume splitByVolumeCommand = new SplitByVolume("a1", "a2", "a3", 0, 0, 31, 19, 400);

            Dictionary<string, Agent> agents = new Dictionary<string, Agent>();
            var a1 = _agentFactory.CreateAgent("a1", 14, 9, 800, 1);

            agents.Add("a1", a1);
            _contaminationService.GetSubstanceId("1");
            _contaminationService.GetSubstanceId("2");
            _contaminationService.GetSubstanceId("3");
            _contaminationService.GetSubstanceId("4");
            _contaminationService.ApplyContamination(a1, contaminationMap);
            _contaminationService.UpdateContaminationArea(contaminationMap, 2, 13, 7, 0, 5);
            _contaminationService.UpdateContaminationArea(contaminationMap, 2, 15, 7, 0, 5);

            Agent splitAgent = a1;
            double agentVolume = splitAgent.Volume;
            double ratio = splitByVolumeCommand.Volume / splitAgent.Volume;

            List<ITemplate> eligibleSplitTemplates = new List<ITemplate>();

            foreach (var template in _templateRepository.SplitTemplates)
            {
                if ((template.MinSize <= agentVolume && agentVolume < template.MaxSize &&
                      Math.Abs(template.Ratio - ratio) < 0.1))
                {

                    if (Math.Abs(template.RatioRelation.First().Value - ratio) < 0.1) // TODO: Should this tolerance be user defined
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                    else
                    {
                        SplitTemplate t = template.DeepCopy();

                        Dictionary<string, (int x, int y)>
                            finalPositions = new Dictionary<string, (int x, int y)>();

                        finalPositions.Add(splitByVolumeCommand.OutputName1, (template.FinalPositions[template.RatioRelation.First().Key].x, template.FinalPositions[template.RatioRelation.First().Key].y));
                        finalPositions.Add(splitByVolumeCommand.OutputName2, (template.FinalPositions[template.RatioRelation.Last().Key].x, template.FinalPositions[template.RatioRelation.Last().Key].y));

                        t.FinalPositions = finalPositions;

                        eligibleSplitTemplates.Add(t);
                    }
                }
            }

            var optimalPosition = _schedulerService.ScheduleCommand(splitByVolumeCommand, agents, contaminationMap, eligibleSplitTemplates);
            Console.WriteLine(optimalPosition);
            Assert.That(10, Is.EqualTo(optimalPosition.X1));
            Assert.That(9, Is.EqualTo(optimalPosition.Y1));
            Assert.That(12, Is.EqualTo(optimalPosition.X2));
            Assert.That(9, Is.EqualTo(optimalPosition.Y2));
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
                    board[i][j] = new Electrode((i + 1) + (j * 32), i, j, 0, 0);
                }
            }
            return board;
        }

    }
}