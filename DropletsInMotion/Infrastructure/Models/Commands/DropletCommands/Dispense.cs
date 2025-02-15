﻿using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Dispense : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DropletName { get; }

        public string ReservoirName { get; }

        public double Volume { get; set; }

        public bool Completed { get; set; } = false;
        public ArithmeticExpression VolumeExpression { get; }

        public Dispense(string name, string reservoirName, double volume)
        {
            DropletName = name;
            ReservoirName = reservoirName;
            Volume = volume;
        }

        public Dispense(string name, string reservoirName, ArithmeticExpression volumeExpression)
        {
            DropletName = name;
            ReservoirName = reservoirName;
            VolumeExpression = volumeExpression;
        }

        public override string ToString()
        {
            return $"Dispense(DropletName: {DropletName}, ReservoirName: {ReservoirName}, Volume: {VolumeExpression})";
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
            Volume = VolumeExpression.Evaluate(variableValues);
        }

        public List<string> GetInputVariables()
        {
            var res = VolumeExpression.GetVariables();
            res.AddRange(GetInputDroplets());
            res.AddRange(new List<string>(){ ReservoirName });
            return res;
        }
        public List<string> GetOutputVariables()
        {
            var res = GetOutputDroplets();
            res.AddRange(new List<string>() { ReservoirName });

            return res;
        }
    }
}
