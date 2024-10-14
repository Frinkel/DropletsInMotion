using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Infrastructure
{
    public static class Debugger
    {
        public static int ExploredStates { get; set; }
        public static int ExistingStates { get; set; }
        public static int ExpandedStates { get; set; }
        public static List<long> ElapsedTime = new List<long>();
    }
}
