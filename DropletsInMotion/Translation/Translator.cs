using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation.Language;
using DropletsInMotion.Presentation.Services;
using DropletsInMotion.Translation.Services;

namespace DropletsInMotion.Translation;

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
    private readonly IContaminationConfigLoader _contaminationConfigLoader;
    private readonly IContaminationRepository _contaminationRepository;

    public Translator(IPlatformService platformService, IUserService userService, IDependencyBuilder dependencyBuilder, IFileService fileService, ITypeChecker typeChecker, IPlatformRepository platformRepository, IContaminationConfigLoader contaminationConfigLoader, IContaminationRepository contaminationRepository)
    {
        _platformService = platformService;
        _userService = userService;
        _dependencyBuilder = dependencyBuilder;
        _fileService = fileService;
        _typeChecker = typeChecker;
        _platformRepository = platformRepository;
        _contaminationConfigLoader = contaminationConfigLoader;
        _contaminationRepository = contaminationRepository;
    }

    public void Load()
    {
        _platformService.Load();
        _contaminationConfigLoader.Load();

        Board = _platformService.Board;
        Agent.SetMinimumMovementVolume(_platformRepository.MinimumMovementVolume);
        Agent.SetMinSize1x1(_platformRepository.MinSize1x1);
        Agent.SetMinSize2x2(_platformRepository.MinSize2x2);
        Agent.SetMinSize3x3(_platformRepository.MinSize3x3);
    }

    public void Translate()
    {
        _contaminationRepository.ResetContaminationSubstances();

        var programContent = _fileService.ReadFileFromPath(_userService.ProgramPath);
        var inputStream = new AntlrInputStream(programContent);
        var lexer = new MicrofluidicsLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new MicrofluidicsParser(commonTokenStream);
        var listener = new MicrofluidicsCustomListener();
        var tree = parser.program();
        ParseTreeWalker.Default.Walk(listener, tree);
        Commands = listener.Commands;
        _typeChecker.ResetTypeEnviroments();
        _typeChecker.TypeCheck(Commands);

        DependencyGraph = _dependencyBuilder.Build(Commands);
    }
}