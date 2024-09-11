using DropletsInMotion.Domain;

namespace DropletsInMotion.Language
{
    public class MicrofluidicsCustomListener : MicrofluidicsBaseListener
    {
        public List<Droplet> Droplets { get; } = new List<Droplet>();
        public List<Move> Moves { get; } = new List<Move>();
        public Stack<int> ExpressionStack { get; } = new Stack<int>();

        public List<ICommand> Commands => new List<ICommand>();

        public override void ExitDropletDeclaration(MicrofluidicsParser.DropletDeclarationContext context)
        {
            string name = context.IDENTIFIER().GetText();
            int positionX = int.Parse(context.INT(0).GetText());
            int positionY = int.Parse(context.INT(1).GetText());
            double volume = double.Parse(context.FLOAT().GetText());

            Droplets.Add(new Droplet(name, positionX, positionY, volume));
            Commands.Add(new Droplet(name, positionX, positionY, volume));
        }

        public override void ExitMoveDroplet(MicrofluidicsParser.MoveDropletContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();
            int newPositionX = int.Parse(context.INT(0).GetText());
            int newPositionY = int.Parse(context.INT(1).GetText());

            Moves.Add(new Move(dropletName, newPositionX, newPositionY));
            Commands.Add(new Move(dropletName, newPositionX, newPositionY));
        }

        // Listener for addition expressions
        public override void ExitAddExpr(MicrofluidicsParser.AddExprContext context)
        {
            // Pop the right and left operand from the stack and add them
            int right = ExpressionStack.Pop();
            int left = ExpressionStack.Pop();
            ExpressionStack.Push(left + right);
        }

        // Listener for integer expressions
        public override void ExitIntExpr(MicrofluidicsParser.IntExprContext context)
        {
            // Push the integer value onto the expression stack
            ExpressionStack.Push(int.Parse(context.INT().GetText()));
        }
    }

}