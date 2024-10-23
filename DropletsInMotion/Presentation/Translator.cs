using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation.Services;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Presentation.Language;
using static MicrofluidicsParser;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;

namespace DropletsInMotion.Presentation
{
    public class Translator : ITranslator
    {
        public Electrode[][] Board { get; set; }
        public List<ICommand> Commands { get; set; }
        public DependencyGraph DependencyGraph { get; set; }

        private readonly IPlatformService _platformService;
        private readonly IUserService _userService;
        private readonly IDependencyBuilder _dependencyBuilder;
        private readonly IFileService _fileService;
        private readonly ITypeChecker _typeChecker;
        private readonly IPlatformRepository _platformRepository;

        public Translator(IPlatformService platformService, IUserService userService, IDependencyBuilder dependencyBuilder, IFileService fileService, ITypeChecker typeChecker, IPlatformRepository platformRepository)
        {
            _platformService = platformService;
            _userService = userService;
            _dependencyBuilder = dependencyBuilder;
            _fileService = fileService;
            _typeChecker = typeChecker;
            _platformRepository = platformRepository;
        }

        public void Load()
        {
            _platformService.Load();
            Board = _platformService.Board;
        }

        public void Translate()
        {
            var programContent = _fileService.ReadFileFromPath(_userService.ProgramPath);
            var inputStream = new AntlrInputStream(programContent);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);
            var listener = new MicrofluidicsCustomListener();
            var tree = parser.program();
            ParseTreeWalker.Default.Walk(listener, tree);
            Commands = listener.Commands;
            _typeChecker.resetTypeEnviroments();
            _typeChecker.typeCheck(Commands);
            Agent.SetMinimumMovementVolume(_platformRepository.MinimumMovementVolume);
            Agent.SetMinSize1x1(_platformRepository.MinSize1x1);
            Agent.SetMinSize2x2(_platformRepository.MinSize2x2);
            Agent.SetMinSize3x3(_platformRepository.MinSize3x3);

            DependencyGraph = _dependencyBuilder.Build(Commands);
        }

    }
    }
