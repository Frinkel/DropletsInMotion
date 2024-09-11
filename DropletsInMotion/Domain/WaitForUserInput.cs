using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class WaitForUserInput : ICommand
    {
        public WaitForUserInput() { }

        public override string ToString()
        {
            return $"WaitForUserInput()";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { };
        }
    }
}
