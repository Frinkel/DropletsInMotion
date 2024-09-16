using DropletsInMotion.Domain;
using static System.Formats.Asn1.AsnWriter;

namespace DropletsInMotion.Language
{
    public class MicrofluidicsCustomListener : MicrofluidicsBaseListener
    {
        public List<ICommand> Commands { get; } = new List<ICommand>();
        public Dictionary<string, Droplet> Droplets { get; } = new Dictionary<string, Droplet>();
        public override void ExitDropletDeclaration(MicrofluidicsParser.DropletDeclarationContext context)
        {
            string name = context.IDENTIFIER().GetText();
            int positionX = int.Parse(context.INT(0).GetText());
            int positionY = int.Parse(context.INT(1).GetText());
            double volume = double.Parse(context.FLOAT().GetText());

            Droplet droplet = new Droplet(name, positionX, positionY, volume);
            Droplets[name] = droplet;
        }

        public override void ExitMoveDroplet(MicrofluidicsParser.MoveDropletContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();
            int newPositionX = int.Parse(context.INT(0).GetText());
            int newPositionY = int.Parse(context.INT(1).GetText());

            ICommand command = new Move(dropletName, newPositionX, newPositionY);
            Commands.Add(command);
        }

        public override void ExitDispense(MicrofluidicsParser.DispenseContext context)
        {
            string name = context.IDENTIFIER(0).GetText();
            string inputName = context.IDENTIFIER(1).GetText();
            double volume = double.Parse(context.FLOAT().GetText());

            ICommand command = new Dispense(name, inputName, volume);
            Commands.Add(command);
        }

        public override void ExitSplitByRatio(MicrofluidicsParser.SplitByRatioContext context)
        {
            string input = context.IDENTIFIER(0).GetText();
            string output1 = context.IDENTIFIER(1).GetText();
            string output2 = context.IDENTIFIER(2).GetText();
            int posX1 = int.Parse(context.INT(0).GetText());
            int posY1 = int.Parse(context.INT(1).GetText());
            int posX2 = int.Parse(context.INT(2).GetText());
            int posY2 = int.Parse(context.INT(3).GetText());
            double ratio = double.Parse(context.FLOAT().GetText());

            ICommand command = new SplitByRatio(input, output1, output2, posX1, posY1, posX2, posY2, ratio);
            Commands.Add(command);
        }

        public override void ExitSplitByVolume(MicrofluidicsParser.SplitByVolumeContext context)
        {
            string input = context.IDENTIFIER(0).GetText();
            string output1 = context.IDENTIFIER(1).GetText();
            string output2 = context.IDENTIFIER(2).GetText();
            int posX1 = int.Parse(context.INT(0).GetText());
            int posY1 = int.Parse(context.INT(1).GetText());
            int posX2 = int.Parse(context.INT(2).GetText());
            int posY2 = int.Parse(context.INT(3).GetText());
            double volume = double.Parse(context.FLOAT().GetText());

            ICommand command = new SplitByVolume(input, output1, output2, posX1, posY1, posX2, posY2, volume);
            Commands.Add(command);
        }

        public override void ExitMerge(MicrofluidicsParser.MergeContext context)
        {
            string input1 = context.IDENTIFIER(0).GetText();
            string input2 = context.IDENTIFIER(1).GetText();
            string output = context.IDENTIFIER(2).GetText();
            int posX = int.Parse(context.INT(0).GetText());
            int posY = int.Parse(context.INT(1).GetText());

            ICommand command = new Merge(input1, input2, output, posX, posY);
            Commands.Add(command);
        }

        public override void ExitMix(MicrofluidicsParser.MixContext context)
        {
            string name = context.IDENTIFIER().GetText();
            int posX = int.Parse(context.INT(0).GetText());
            int posY = int.Parse(context.INT(1).GetText());
            int distanceX = int.Parse(context.INT(2).GetText());
            int distanceY = int.Parse(context.INT(3).GetText());
            int repeatTimes = int.Parse(context.INT(4).GetText());

            ICommand command = new Mix(name, posX, posY, distanceX, distanceY, repeatTimes);
            Commands.Add(command);
        }

        public override void ExitStore(MicrofluidicsParser.StoreContext context)
        {
            string name = context.IDENTIFIER().GetText();
            int posX = int.Parse(context.INT(0).GetText());
            int posY = int.Parse(context.INT(1).GetText());
            int time = int.Parse(context.INT(2).GetText());

            ICommand command = new Store(name, posX, posY, time);
            Commands.Add(command);
        }

        public override void ExitWait(MicrofluidicsParser.WaitContext context)
        {
            int time = int.Parse(context.INT().GetText());

            ICommand command = new Wait(time);
            Commands.Add(command);
        }

        public override void ExitWaitForUserInput(MicrofluidicsParser.WaitForUserInputContext context)
        {
            ICommand command = new WaitForUserInput();
            Commands.Add(command);
        }
    }
}