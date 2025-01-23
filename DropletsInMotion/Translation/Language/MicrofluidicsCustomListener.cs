using Antlr4.Runtime.Tree;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;
using System.Globalization;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;

namespace DropletsInMotion.Presentation.Language
{
    public class MicrofluidicsCustomListener : MicrofluidicsBaseListener
    {
        public List<ICommand> Commands { get; } = new List<ICommand>();

        private ArithmeticExpression CreateExpression(MicrofluidicsParser.ArithmeticExpressionContext context)
        {
            // Binary operation (e.g., +, -, *, /)
            if (context.op != null && context.arithmeticExpression().Length == 2)
            {
                ArithmeticExpression left = CreateExpression(context.arithmeticExpression(0));
                ArithmeticExpression right = CreateExpression(context.arithmeticExpression(1));
                string operatorSymbol = context.op.Text;

                return new BinaryArithmeticExpression(left, operatorSymbol, right);
            }

            // Unary minus operation
            if (context.GetText().StartsWith("-") && context.arithmeticExpression().Length == 1)
            {
                ArithmeticExpression operand = CreateExpression(context.arithmeticExpression(0));
                return new UnaryNegationExpression(operand);
            }

            // Integer literal
            if (context.INT() != null)
            {
                int value = int.Parse(context.INT().GetText());
                return new LiteralExpression(value);
            }

            // Floating point literal
            if (context.FLOAT() != null)
            {
                double value = double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
                return new LiteralExpression(value);
            }

            // Variable (identifier)
            if (context.IDENTIFIER() != null)
            {
                string variableName = context.IDENTIFIER().GetText();
                return new VariableExpression(variableName);
            }

            // Handle parentheses by evaluating the expression inside
            if (context.arithmeticExpression().Length == 1)
            {
                return CreateExpression(context.arithmeticExpression(0));
            }

            throw new InvalidOperationException("Unknown arithmetic expression structure.");
        }


        public override void ExitDropletDeclaration(MicrofluidicsParser.DropletDeclarationContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();

            var positionXExpression = CreateExpression(context.arithmeticExpression(0));
            var positionYExpression = CreateExpression(context.arithmeticExpression(1));
            var volumeExpression = CreateExpression(context.arithmeticExpression(2));

            string substance = context.STRING() != null
                ? context.STRING().GetText().Trim('"')
                : "";

            IDropletCommand dropletDeclaration = new DropletDeclaration(
                dropletName,
                positionXExpression,
                positionYExpression,
                volumeExpression,
                substance
            );
            dropletDeclaration.Line = context.Start.Line;
            dropletDeclaration.Column = context.Start.Column;
            Commands.Add(dropletDeclaration);
        }



        public override void ExitMoveDroplet(MicrofluidicsParser.MoveDropletContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();
            var newPositionX = CreateExpression(context.arithmeticExpression(0));
            var newPositionY = CreateExpression(context.arithmeticExpression(1));

            IDropletCommand dropletCommand = new Move(dropletName, newPositionX, newPositionY);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitDispenserDeclaration(MicrofluidicsParser.DispenserDeclarationContext context)
        {
            string dispenserIdentifer = context.IDENTIFIER().GetText();
            string dispenserName = context.STRING(0).GetText().Trim('"');
            string substance = context.STRING(1).GetText().Trim('"');

            IDropletCommand declareDispenserCommand = new DeclareDispenserCommand(dispenserIdentifer, dispenserName, substance);
            declareDispenserCommand.Line = context.Start.Line;
            declareDispenserCommand.Column = context.Start.Column;
            Commands.Add(declareDispenserCommand);
        }

        public override void ExitDispense(MicrofluidicsParser.DispenseContext context)
        {
            string dropletName = context.IDENTIFIER(0).GetText();
            string reservoirName = context.IDENTIFIER(1).GetText();
            var volume = CreateExpression(context.arithmeticExpression());

            IDropletCommand dropletCommand = new Dispense(dropletName, reservoirName, volume);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitSplitByRatio(MicrofluidicsParser.SplitByRatioContext context)
        {
            string input = context.IDENTIFIER(0).GetText();
            string output1 = context.IDENTIFIER(1).GetText();
            string output2 = context.IDENTIFIER(2).GetText();
            var posX1 = CreateExpression(context.arithmeticExpression(0));
            var posY1 = CreateExpression(context.arithmeticExpression(1));
            var posX2 = CreateExpression(context.arithmeticExpression(2));
            var posY2 = CreateExpression(context.arithmeticExpression(3));
            var ratio = CreateExpression(context.arithmeticExpression(4));

            IDropletCommand dropletCommand = new SplitByRatio(input, output1, output2, posX1, posY1, posX2, posY2, ratio);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitSplit(MicrofluidicsParser.SplitContext context)
        {
            string input = context.IDENTIFIER(0).GetText();
            string output1 = context.IDENTIFIER(1).GetText();
            string output2 = context.IDENTIFIER(2).GetText();
            var posX1 = CreateExpression(context.arithmeticExpression(0));
            var posY1 = CreateExpression(context.arithmeticExpression(1));
            var posX2 = CreateExpression(context.arithmeticExpression(2));
            var posY2 = CreateExpression(context.arithmeticExpression(3));
            var ratio = CreateExpression(context.arithmeticExpression(4));

            IDropletCommand dropletCommand = new SplitByRatio(input, output1, output2, posX1, posY1, posX2, posY2, ratio);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }


        public override void ExitSplitByVolume(MicrofluidicsParser.SplitByVolumeContext context)
        {
            string input = context.IDENTIFIER(0).GetText();
            string output1 = context.IDENTIFIER(1).GetText();
            string output2 = context.IDENTIFIER(2).GetText();
            var posX1 = CreateExpression(context.arithmeticExpression(0));
            var posY1 = CreateExpression(context.arithmeticExpression(1));
            var posX2 = CreateExpression(context.arithmeticExpression(2));
            var posY2 = CreateExpression(context.arithmeticExpression(3));
            var volume = CreateExpression(context.arithmeticExpression(4));

            IDropletCommand dropletCommand = new SplitByVolume(input, output1, output2, posX1, posY1, posX2, posY2, volume);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitMerge(MicrofluidicsParser.MergeContext context)
        {
            string input1 = context.IDENTIFIER(0).GetText();
            string input2 = context.IDENTIFIER(1).GetText();
            string output = context.IDENTIFIER(2).GetText();
            var posX = CreateExpression(context.arithmeticExpression(0));
            var posY = CreateExpression(context.arithmeticExpression(1));

            IDropletCommand dropletCommand = new Merge(input1, input2, output, posX, posY);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitMix(MicrofluidicsParser.MixContext context)
        {
            string name = context.IDENTIFIER().GetText();
            var posX = CreateExpression(context.arithmeticExpression(0));
            var posY = CreateExpression(context.arithmeticExpression(1));
            var distanceX = CreateExpression(context.arithmeticExpression(2));
            var distanceY = CreateExpression(context.arithmeticExpression(3));
            var repeatTimes = CreateExpression(context.arithmeticExpression(4));

            IDropletCommand dropletCommand = new Mix(name, posX, posY, distanceX, distanceY, repeatTimes);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitStoreWithPositions(MicrofluidicsParser.StoreWithPositionsContext context)
        {
            string name = context.IDENTIFIER().GetText();
            var posX = CreateExpression(context.arithmeticExpression(0));
            var posY = CreateExpression(context.arithmeticExpression(1));
            var time = CreateExpression(context.arithmeticExpression(2));

            IDropletCommand dropletCommand = new Store(name, posX, posY, time);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitStoreWithTimeOnly(MicrofluidicsParser.StoreWithTimeOnlyContext context)
        {
            string name = context.IDENTIFIER().GetText();
            var time = CreateExpression(context.arithmeticExpression());

            Commands.Add(new Store(name, time));
        }

        public override void ExitWaste(MicrofluidicsParser.WasteContext context)
        {
            string name = context.IDENTIFIER().GetText();
            var posX = CreateExpression(context.arithmeticExpression(0));
            var posY = CreateExpression(context.arithmeticExpression(1));

            IDropletCommand dropletCommand = new Waste(name, posX, posY);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitWait(MicrofluidicsParser.WaitContext context)
        {
            var time = CreateExpression(context.arithmeticExpression());

            IDropletCommand dropletCommand = new Wait(time);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitAssignment(MicrofluidicsParser.AssignmentContext context)
        {
            string variableName = context.IDENTIFIER().GetText();

            ArithmeticExpression valueExpression = CreateExpression(context.arithmeticExpression());

            ICommand assignCommand = new AssignCommand(variableName, valueExpression);
            assignCommand.Line = context.Start.Line;
            assignCommand.Column = context.Start.Column;
            Commands.Add(assignCommand);
        }

        public override void ExitWaitForUserInput(MicrofluidicsParser.WaitForUserInputContext context)
        {
            IDropletCommand dropletCommand = new WaitForUserInput();
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitSensorCommand(MicrofluidicsParser.SensorCommandContext context)
        {
            string variableName = context.IDENTIFIER(0).GetText();
            string sensorName = context.STRING(0).GetText().Trim('"');
            string argument = context.STRING(1).GetText().Trim('"');
            string dropletName = context.IDENTIFIER(1).GetText();

            IDropletCommand dropletCommand = new SensorCommand(variableName, sensorName, argument, dropletName);
            dropletCommand.Line = context.Start.Line;
            dropletCommand.Column = context.Start.Column;
            Commands.Add(dropletCommand);
        }

        public override void ExitPrintStatement(MicrofluidicsParser.PrintStatementContext context)
        {
            List<object> arguments = new List<object>();

            foreach (var arg in context.printArgument())
            {
                if (arg.STRING() != null)
                {
                    arguments.Add(arg.STRING().GetText().Trim('"'));
                }
                else if (arg.arithmeticExpression() != null)
                {
                    arguments.Add(CreateExpression(arg.arithmeticExpression()));
                }
                else if (arg.booleanExpression() != null)
                {
                    arguments.Add(CreateBooleanExpression(arg.booleanExpression()));
                }
            }

            ICommand printCommand = new PrintCommand(arguments);
            printCommand.Line = context.Start.Line;
            printCommand.Column = context.Start.Column;
            Commands.Add(printCommand);
        }

        public override void ExitActuatorCommand(MicrofluidicsParser.ActuatorCommandContext context)
        {
            string identifier = null;
            string actuatorName = context.STRING().GetText().Trim('"');

            if (context.IDENTIFIER() != null)
            {
                identifier = context.IDENTIFIER().GetText();
            }

            Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();

            foreach (var kvpContext in context.argumentKeyValuePair())
            {
                string key = kvpContext.IDENTIFIER().GetText();
                if (!Double.TryParse(kvpContext.arithmeticExpression().GetText(), out double value))
                {
                    throw new Exception($"Actuator {actuatorName} had an invalid argument format on {key}");
                }

                keyValuePairs.Add(key, value);
            }

            Console.WriteLine(keyValuePairs);

            IDropletCommand actuatorCommand = new ActuatorCommand(identifier, actuatorName, keyValuePairs);
            actuatorCommand.Line = context.Start.Line;
            actuatorCommand.Column = context.Start.Column;
            Commands.Add(actuatorCommand);
        }

        public override void ExitIfStatement(MicrofluidicsParser.IfStatementContext context)
        {
            // Extract the condition (boolean expression)
            BooleanExpression condition = CreateBooleanExpression(context.booleanExpression());

            // Extract commands from the then block
            var thenCommands = ExtractCommandsFromBlock(context.block(0));

            // Extract commands from the else block, if it exists
            List<ICommand> elseCommands = new List<ICommand>();
            if (context.block().Length > 1)
            {
                elseCommands = ExtractCommandsFromBlock(context.block(1));
            }

            // Create the IfCommand and add it to the Commands list
            ICommand ifCommand = new IfCommand(condition, thenCommands, elseCommands);
            ifCommand.Line = context.Start.Line;
            ifCommand.Column = context.Start.Column;
            Commands.Add(ifCommand);
        }


        public override void ExitWhileLoop(MicrofluidicsParser.WhileLoopContext context)
        {
            BooleanExpression condition = CreateBooleanExpression(context.booleanExpression());

            // While loop block
            List<ICommand> blockCommands = ExtractCommandsFromBlock(context.block());

            // Create the WhileCommand
            ICommand whileCommand = new WhileCommand(condition, blockCommands);
            whileCommand.Line = context.Start.Line;
            whileCommand.Column = context.Start.Column;
            Commands.Add(whileCommand);
        }


        private List<ICommand> ExtractCommandsFromBlock(MicrofluidicsParser.BlockContext context)
        {
            List<ICommand> blockCommands = new List<ICommand>();

            int initialGlobalCommandCount = Commands.Count;

            foreach (var commandCtx in context.command())
            {
                ParseTreeWalker.Default.Walk(this, commandCtx);
            }


            for (int i = initialGlobalCommandCount; i < Commands.Count; i++)
            {
                blockCommands.Add(Commands[i]);
            }

            Commands.RemoveRange(initialGlobalCommandCount - blockCommands.Count, blockCommands.Count * 2);

            return blockCommands;
        }


        private BooleanExpression CreateBooleanExpression(MicrofluidicsParser.BooleanExpressionContext context)
        {

            if (context.booleanExpression().Length == 2 && context.op != null)
            {
                var left = CreateBooleanExpression(context.booleanExpression(0));
                var right = CreateBooleanExpression(context.booleanExpression(1));
                string operatorSymbol = context.op.Text;
                return new LogicalBinaryExpression(left, operatorSymbol, right);
            }

            if (context.GetText().StartsWith("!") &&  context.booleanExpression().Length == 1)
            {
                BooleanExpression innerExpression = CreateBooleanExpression(context.booleanExpression(0));
                return new LogicalNotExpression(innerExpression);
            }

            if (context.booleanExpression().Length == 1)
            {
                return CreateBooleanExpression(context.booleanExpression(0));
            }

            if (context.arithmeticExpression().Length == 2)
            {
                var left = CreateExpression(context.arithmeticExpression(0));
                var right = CreateExpression(context.arithmeticExpression(1));
                string operatorSymbol = context.op.Text;
                return new ComparisonExpression(left, operatorSymbol, right);
            }

            throw new InvalidOperationException("Unknown boolean expression structure.");
        }
    }
}