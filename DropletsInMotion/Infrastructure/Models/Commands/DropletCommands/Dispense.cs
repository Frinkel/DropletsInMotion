using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class Dispense : IDropletCommand
    {
        public string DropletName { get; }

        public string InputName { get; }

        public double Volume { get; set; }
        public ArithmeticExpression VolumeExpression { get; }

        public Dispense(string name, string inputName, double volume)
        {
            DropletName = name;
            InputName = inputName;
            Volume = volume;
        }

        public Dispense(string name, string inputName, ArithmeticExpression volumeExpression)
        {
            DropletName = name;
            InputName = inputName;
            VolumeExpression = volumeExpression;
        }

        public override string ToString()
        {
            return $"Dispense(DropletName: {DropletName}, InputName: {InputName}, Volume: {VolumeExpression})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            Volume = VolumeExpression.Evaluate(variableValues);
        }
    }
}
