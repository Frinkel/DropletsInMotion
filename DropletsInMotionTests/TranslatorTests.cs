using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Services;
using DropletsInMotion.Presentation;
using DropletsInMotion.Translation;
using Microsoft.Extensions.DependencyInjection;

namespace DropletsInMotionTests
{
    public class TranslatorTests : TestBase
    {
        private readonly ITranslator _translator;
        private readonly IUserService _userService;
        private readonly IFileService _filerService;

        private string projectDirectory;
        private string platformPath;

        public TranslatorTests()
        {
            _translator = ServiceProvider.GetRequiredService<ITranslator>();
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _filerService = ServiceProvider.GetRequiredService<IFileService>();
            _userService.ConfigurationPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/Configuration";

        }

        //[Test]
        //public void ParseBoardTest()
        //{

        //    _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
        //    _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMoveDroplet.txt";

        //    _translator.Translate();

        //    Assert.That(_translator.Board.Length, Is.EqualTo(32));
        //    Assert.That(_translator.Board[0].Length, Is.EqualTo(20));
        //}



        //[Test]
        //public async Task MoveOneDropletTest()
        //{
        //    _userService.PlatformPath = _filerService.GetProjectDirectory() + "/Assets/Configurations/platform.json";
        //    _userService.ProgramPath = _filerService.GetProjectDirectory() + "/Assets/Programs/TestMoveDroplet.txt";

        //    await _executionEngine.Execute();

        //    // Assertions
        //    Assert.That(_executionEngine.Agents.Count, Is.EqualTo(1));
        //    Assert.That(_executionEngine.Agents["d1"].PositionX, Is.EqualTo(3));
        //    Assert.That(_executionEngine.Agents["d1"].PositionY, Is.EqualTo(3));
        //}


        //[Test]
        //public void ParserTest1()
        //{
        //    String contents = "Droplet(d1, 15, 15, 0.2);\r\nMove(d1, 3, 3);\r\n";
        //    var inputStream = new AntlrInputStream(contents);
        //    var lexer = new MicrofluidicsLexer(inputStream);
        //    var commonTokenStream = new CommonTokenStream(lexer);
        //    var parser = new MicrofluidicsParser(commonTokenStream);

        //    // Get the root of the parse tree (starting with 'program')
        //    var tree = parser.program();
        //    Assert.AreEqual(tree.ToStringTree(parser), "(program (dropletCommand (dropletDeclaration Droplet ( d1 , 15 , 15 , 0.2 ))) ; (dropletCommand (moveDroplet Move ( d1 , 3 , 3 ))) ; <EOF>)");
        //}

        //[Test]
        //public void TemplateTester()
        //{
        //    var board = new Electrode[32][];
        //    for (int i = 0; i < 32; i++)
        //    {
        //        board[i] = new Electrode[20];
        //        for (int j = 0; j < 20; j++)
        //        {
        //            board[i][j] = new Electrode((i + 1) + (j * 32), i, j);
        //        }
        //    }

        //    TemplateService templateHandler = new TemplateService();
        //    templateHandler.Initialize(board);
        //    List<(string, List<BoardAction>)> templates = templateHandler.Templates;
        //    Console.WriteLine(templates);
        //    Assert.AreEqual(templates.Count, 9);
        //    Assert.AreEqual(templates[0].Item1, "mergeHorizontal");

        //}
    }
}