﻿using DropletsInMotion.Domain;

namespace DropletsInMotion.Language
{
    public class MicrofluidicsCustomListener : MicrofluidicsBaseListener
    {
        public List<Droplet> Droplets { get; } = new List<Droplet>();
        public List<Move> Moves { get; } = new List<Move>();

        public override void ExitDropletDeclaration(MicrofluidicsParser.DropletDeclarationContext context)
        {
            string name = context.IDENTIFIER().GetText();
            int positionX = int.Parse(context.INT(0).GetText());
            int positionY = int.Parse(context.INT(1).GetText());
            double volume = double.Parse(context.FLOAT().GetText());

            Droplets.Add(new Droplet(name, positionX, positionY, volume));
        }

        public override void ExitMoveDroplet(MicrofluidicsParser.MoveDropletContext context)
        {
            string dropletName = context.IDENTIFIER().GetText();
            int newPositionX = int.Parse(context.INT(0).GetText());
            int newPositionY = int.Parse(context.INT(1).GetText());

            Moves.Add(new Move(dropletName, newPositionX, newPositionY));
        }
    }

}