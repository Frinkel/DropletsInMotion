using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class Dispense : ICommand
    {
        public string Name { get; }

        public string InputName { get; }
    
        public double Volume { get; set; }

        public Dispense(string name, string inputName, double volume)
        {
            Name = name;
            InputName = inputName;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"Droplet(Name: {Name}, InputName: {InputName}, Volume: {Volume})";
        }
    }
}
