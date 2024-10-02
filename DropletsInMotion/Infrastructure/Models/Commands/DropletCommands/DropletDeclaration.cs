using DropletsInMotion.Infrastructure.Models.Commands.Expressions;
using System.Collections.Generic;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class DropletDeclaration : IDropletCommand
    {
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Volume { get; set; }

        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }
        public ArithmeticExpression VolumeExpression { get; }

        public DropletDeclaration(string dropletName, int positionX, int positionY, double volume)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
            Volume = volume;
        }

        public DropletDeclaration(string dropletName, ArithmeticExpression positionXExpression, ArithmeticExpression positionYExpression, ArithmeticExpression volumeExpression)
        {
            DropletName = dropletName;
            PositionXExpression = positionXExpression;
            PositionYExpression = positionYExpression;
            VolumeExpression = volumeExpression;
        }

        public override string ToString()
        {
            return $"DropletDeclaration(DropletName: {DropletName}, PositionX: {PositionXExpression}, PositionY: {PositionYExpression}, Volume: {VolumeExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string>();
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            PositionX = (int)PositionXExpression.Evaluate(variableValues);
            PositionY = (int)PositionYExpression.Evaluate(variableValues);
            Volume = VolumeExpression.Evaluate(variableValues);
        }
    }
}
