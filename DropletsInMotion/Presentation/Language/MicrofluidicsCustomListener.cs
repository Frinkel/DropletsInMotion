using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;
using DropletsInMotion.Infrastructure.Models.Domain;
using System.Globalization;

namespace DropletsInMotion.Presentation.Language
{
    public class MicrofluidicsCustomListener : MicrofluidicsBaseListener
    {
        public List<ICommand> Commands { get; } = new List<ICommand>();

        private ArithmeticExpression CreateExpression(MicrofluidicsParser.ArithmeticExpressionContext context)
        {
            // If it's a binary operation
            if (context.op != null)
            {
                ArithmeticExpression left = CreateExpression(context.arithmeticExpression(0)); // Left-hand side expression
                ArithmeticExpression right = CreateExpression(context.arithmeticExpression(1)); // Right-hand side expression
                string operatorSymbol = context.op.Text; // The operator, e.g., +, -, *, /

                // Return a BinaryArithmeticExpression (which handles +, -, *, /)
                return new BinaryArithmeticExpression(left, operatorSymbol, right);
            }

            // If it's a unary minus operation
            if (context.op != null && context.op.Text == "-")
            {
                ArithmeticExpression operand = CreateExpression(context.arithmeticExpression(0));
                return new UnaryNegationExpression(operand);
            }

            if (context.INT() != null)
            {
                int value = int.Parse(context.INT().GetText());
                return new LiteralExpression(value);
            }

            if (context.FLOAT() != null)
            {
                double value = double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
                return new LiteralExpression(value);
            }

            if (context.IDENTIFIER() != null)
            {
                string variableName = context.IDENTIFIER().GetText();
                return new VariableExpression(variableName);
            }

            if (context.arithmeticExpression().Length == 1)
            {
                return CreateExpression(context.arithmeticExpression(0)); // Handle parentheses by evaluating what's inside
            }

            throw new InvalidOperationException("Unknown arithmetic expression structure.");
        }

        public override void ExitDropletDeclaration(MicrofluidicsParser.DropletDeclarationContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();

            var positionXExpression = CreateExpression(context.arithmeticExpression(0));
            var positionYExpression = CreateExpression(context.arithmeticExpression(1));
            var volumeExpression = CreateExpression(context.arithmeticExpression(2));

            IDropletCommand dropletDeclaration = new DropletDeclaration(dropletName, positionXExpression, positionYExpression, volumeExpression);

            Commands.Add(dropletDeclaration);
        }


        public override void ExitMoveDroplet(MicrofluidicsParser.MoveDropletContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();
            var newPositionX = CreateExpression(context.arithmeticExpression(0));
            var newPositionY = CreateExpression(context.arithmeticExpression(1));

            IDropletCommand dropletCommand = new Move(dropletName, newPositionX, newPositionY);
            Commands.Add(dropletCommand);
        }

        public override void ExitDispense(MicrofluidicsParser.DispenseContext context)
        {
            string name = context.IDENTIFIER(0).GetText();
            string inputName = context.IDENTIFIER(1).GetText();
            var volume = CreateExpression(context.arithmeticExpression());

            IDropletCommand dropletCommand = new Dispense(name, inputName, volume);
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
            Commands.Add(dropletCommand);
        }

        public override void ExitStore(MicrofluidicsParser.StoreContext context)
        {
            string name = context.IDENTIFIER().GetText();
            var posX = CreateExpression(context.arithmeticExpression(0));
            var posY = CreateExpression(context.arithmeticExpression(1));
            var time = CreateExpression(context.arithmeticExpression(2));

            IDropletCommand dropletCommand = new Store(name, posX, posY, time);
            Commands.Add(dropletCommand);
        }

        public override void ExitWait(MicrofluidicsParser.WaitContext context)
        {
            var time = CreateExpression(context.arithmeticExpression());

            IDropletCommand dropletCommand = new Wait(time);
            Commands.Add(dropletCommand);
        }

        public override void ExitAssignment(MicrofluidicsParser.AssignmentContext context)
        {
            string variableName = context.IDENTIFIER().GetText();

            ArithmeticExpression valueExpression = CreateExpression(context.arithmeticExpression());

            ICommand assignCommand = new Assign(variableName, valueExpression);
            Commands.Add(assignCommand);
        }

        public override void ExitWaitForUserInput(MicrofluidicsParser.WaitForUserInputContext context)
        {
            IDropletCommand dropletCommand = new WaitForUserInput();
            Commands.Add(dropletCommand);
        }

    }
}