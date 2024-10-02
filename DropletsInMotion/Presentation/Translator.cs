using DropletsInMotion.Infrastructure.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation.Services;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using DropletsInMotion.Presentation.Language;
using static MicrofluidicsParser;
using DropletsInMotion.Infrastructure.Models;

namespace DropletsInMotion.Presentation
{
    public class Translator : ITranslator
    {
        public Electrode[][] Board { get; set; }
        public Dictionary<string, Droplet> Droplets { get; set; }
        public List<ICommand> Commands { get; set; }
        public DependencyGraph DependencyGraph { get; set; }

        private readonly IPlatformService _platformService;
        private readonly IUserService _userService;
        private readonly IDependencyBuilder _dependencyBuilder;
        private readonly IFileService _fileService;

        public Translator(IPlatformService platformService, IUserService userService, IDependencyBuilder dependencyBuilder, IFileService fileService)
        {
            _platformService = platformService;
            _userService = userService;
            _dependencyBuilder = dependencyBuilder;
            _fileService = fileService;

            tranlate();

        }

        private void tranlate()
        {
            _platformService.LoadBoardFromJson(_userService.PlatformPath);
            Board = _platformService.Board;

            var programContent = _fileService.ReadFileFromPath(_userService.ProgramPath);
            var inputStream = new AntlrInputStream(programContent);
            var lexer = new MicrofluidicsLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new MicrofluidicsParser(commonTokenStream);
            var listener = new MicrofluidicsCustomListener();
            var tree = parser.program();
            ParseTreeWalker.Default.Walk(listener, tree);
            Commands = listener.Commands;
            Droplets = listener.Droplets;
            DependencyGraph = _dependencyBuilder.Build(Commands);
        }

    }
    }
