using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class Dispense : ICommand
    {
        public string DropletName { get; }

        public string InputName { get; }
    
        public double Volume { get; set; }

        public Dispense(string name, string inputName, double volume)
        {
            DropletName = name;
            InputName = inputName;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"Dispense(DropletName: {DropletName}, InputName: {InputName}, Volume: {Volume})";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { DropletName };
        }
    }
}
