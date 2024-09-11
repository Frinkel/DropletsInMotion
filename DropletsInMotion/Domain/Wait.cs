using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public class Wait : ICommand
    {
        public double Time { get; }

        public Wait(double time)
        {
            Time = time;
        }

        public override string ToString()
        {
            return $"Wait(Time: {Time})";
        }
    }
}
