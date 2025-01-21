
using Antlr4.Runtime;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Presentation.Services;
using DropletsInMotion.Application.Services;
using DropletsInMotion.Application.Execution;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation;
using Microsoft.Extensions.DependencyInjection;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotionTests
{
    public class DependencyTest : TestBase
    {
        private readonly ITranslator _translator;
        private readonly IUserService _userService;
        private readonly IFileService _filerService;

        private string projectDirectory;
        private string platformPath;

        public DependencyTest()
        {
            _translator = ServiceProvider.GetRequiredService<ITranslator>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
            _userService.ConfigurationPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/Configuration";

        }

        [Test]
        public void NoDependecyTest()
        {
            var commands = new List<ICommand>
            {
                new DropletDeclaration("d1", new LiteralExpression(1), new LiteralExpression(1), new LiteralExpression(500), "H2O"),
                new DropletDeclaration("d2", new LiteralExpression(10), new LiteralExpression(1), new LiteralExpression(500), "H2O"),
            };

            var builder = new DependencyBuilder();
            var graph = builder.Build(commands);

            foreach (var node in graph.GetAllNodes())
            {
                Assert.IsEmpty(node.Dependencies);
            }
        }


        [Test]
        public void HasDependencyTest()
        {
            var commands = new List<ICommand>
            {
                new DropletDeclaration("d1", new LiteralExpression(1), new LiteralExpression(1), new LiteralExpression(500), "H2O"),
                new Move("d1", new LiteralExpression(10), new LiteralExpression(1))
            };

            var builder = new DependencyBuilder();
            var graph = builder.Build(commands);

            var nodes = graph.GetAllNodes();

            Assert.Contains(nodes[0], nodes[1].Dependencies.ToList());
            Assert.IsEmpty(nodes[0].Dependencies);
        }


        [Test]
        public void DependencyIfStatementTest()
        {
            var thenCommands = new List<ICommand>
            {
                new AssignCommand("a", new LiteralExpression(5))
            };
            var elseCommands = new List<ICommand>
            {
                new AssignCommand("b", new LiteralExpression(10))
            };

            var commands = new List<ICommand>
            {
                new IfCommand(new ComparisonExpression(new VariableExpression("x"), ">", new LiteralExpression(0)), thenCommands, elseCommands),
                new AssignCommand("c", new VariableExpression("a"))
            };

            var builder = new DependencyBuilder();
            var graph = builder.Build(commands);

            var ifNode = graph.GetAllNodes().OfType<DependencyNodeIf>().FirstOrDefault();
            Assert.AreEqual(1, ifNode.ThenBody.GetAllNodes().Count);
            Assert.AreEqual(1, ifNode.ElseBody.GetAllNodes().Count);

            var nodes = graph.GetAllNodes();
            Assert.Contains(ifNode, nodes[1].Dependencies.ToList());
        }

        [Test]
        public void DependencyGraph_WhileLoop_SubgraphCreated()
        {
            var loopCommands = new List<ICommand>
            {
                new AssignCommand("a", new LiteralExpression(5))
            };
            var commands = new List<ICommand>
            {
                new WhileCommand(new ComparisonExpression(new VariableExpression("x"), ">", new LiteralExpression(0)), loopCommands),
                new AssignCommand("b", new VariableExpression("a"))
            };

            var builder = new DependencyBuilder();
            var graph = builder.Build(commands);

            var whileNode = graph.GetAllNodes().OfType<DependencyNodeWhile>().FirstOrDefault();
            Assert.AreEqual(1, whileNode.Body.GetAllNodes().Count);
        }

    }
}