﻿using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class DropletDeclaration : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DropletName { get; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public double Volume { get; set; }
        public string Substance { get; set; }

        public ArithmeticExpression PositionXExpression { get; }
        public ArithmeticExpression PositionYExpression { get; }
        public ArithmeticExpression VolumeExpression { get; }

        public DropletDeclaration(string dropletName, int positionX, int positionY, double volume, string substance)
        {
            DropletName = dropletName;
            PositionX = positionX;
            PositionY = positionY;
            Volume = volume;
            Substance = substance;
        }

        public DropletDeclaration(string dropletName, ArithmeticExpression positionXExpression, ArithmeticExpression positionYExpression, ArithmeticExpression volumeExpression, string substance)
        {
            DropletName = dropletName;
            PositionXExpression = positionXExpression;
            PositionYExpression = positionYExpression;
            VolumeExpression = volumeExpression;
            Substance = substance;
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

        public List<string> GetVariables()
        {
            var x = PositionXExpression.GetVariables();
            x.AddRange(PositionYExpression.GetVariables());
            x.AddRange(VolumeExpression.GetVariables());
            return x;
        }

        public List<string> GetInputVariables()
        {
            return GetVariables();
        }

        public List<string> GetOutputVariables() {
            return GetOutputDroplets();
        }
    }
}
