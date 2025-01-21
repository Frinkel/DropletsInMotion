using Antlr4.Runtime;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Presentation.Services;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Application.Factories;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Translation.Services;

namespace DropletsInMotionTests
{
    public class ContaminationTests : TestBase
    {
        private readonly IUserService _userService;
        private readonly IFileService _filerService;
        private readonly IContaminationService _contaminationService;
        private readonly IAgentFactory _agentFactory;
        private readonly IContaminationRepository _contaminationRepository;


        private string projectDirectory;
        private string platformPath;

        public ContaminationTests()
        {
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
            _userService.ConfigurationPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/Configuration";
            _contaminationService = ServiceProvider.GetRequiredService<IContaminationService>();
            _agentFactory = ServiceProvider.GetRequiredService<IAgentFactory>();
            _contaminationRepository = ServiceProvider.GetRequiredService<IContaminationRepository>();

        }


        [Test]
        public void ApplycontaminationTest()
        {   


            var agent = _agentFactory.CreateAgent("a1", 2, 2, 706, 1);
            var contaminationMap = _contaminationService.CreateContaminationMap(32, 20);

            _contaminationService.ApplyContamination(agent, contaminationMap);

            var expectedPositions = new List<(int x, int y)>
            {
                (2, 2), (3, 2), (1, 2), (2, 3), (2, 1),
                (3, 3), (3, 1), (1, 3), (1, 1)
            };

            foreach (var (x, y) in expectedPositions)
            {
                Assert.Contains(agent.SubstanceId, contaminationMap[x, y]);
            }
        }

        [Test]
        public void IsConflictingDefaultTest()
        {
            _contaminationRepository.SubstanceTable = new List<(string, bool)>
            {
                ("s1", false),
                ("s2", false)
            };

            var contaminationMap = _contaminationService.CreateContaminationMap(32, 20);
            contaminationMap[1, 1].Add(0);

            bool conflict = _contaminationService.IsConflicting(contaminationMap, 1, 1, 1);

            Assert.IsTrue(conflict);
        }

        [Test]
        public void IsConflictingContaminationTableTrueTest()
        {
            _contaminationRepository.SubstanceTable = new List<(string, bool)>
            {
                ("s1", true),
                ("s2", true)
            };

            _contaminationRepository.ContaminationTable = new List<List<bool>>
            {
                new List<bool> { false, true },
                new List<bool> { true, false }
            };

            var contaminationMap = _contaminationService.CreateContaminationMap(32, 20);
            contaminationMap[1, 1].Add(0);

            bool conflict = _contaminationService.IsConflicting(contaminationMap, 1, 1, 1);

            Assert.IsTrue(conflict);
        }


        [Test]
        public void IsConflictingContaminationTableFalseTest()
        {
            _contaminationRepository.SubstanceTable = new List<(string, bool)>
            {
                ("s1", true),
                ("s2", true)
            };

            _contaminationRepository.ContaminationTable = new List<List<bool>>
            {
                new List<bool> { false, true },
                new List<bool> { false, false }
            };

            var contaminationMap = _contaminationService.CreateContaminationMap(32, 20);
            contaminationMap[1, 1].Add(0);

            bool conflict = _contaminationService.IsConflicting(contaminationMap, 1, 1, 1);

            Assert.IsFalse(conflict);
        }

    }
}