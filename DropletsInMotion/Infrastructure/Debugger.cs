using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
        
        public static List<(int x, int y)> Nodes = new List<(int x, int y)>();

        public static int Permutations { get; set; }

        private static double _prevMemory = 0;

        public static Process GetProcess()
        {
            var process = Process.GetCurrentProcess();
            return process;
        }

        public static void PrintMemoryUsage(Process process)
        {
            process.Refresh();
            var memory = process.WorkingSet64 / (1024.0 * 1024.0);
            Console.WriteLine($"Current memory '{process.ProcessName}': {memory - _prevMemory} MB");
            _prevMemory = memory;
        }


        public static void PrintDuplicateCounts()
        {
            // Group by unique entries and count each occurrence
            var duplicatesCount = Nodes
                .GroupBy(p => p)
                .ToDictionary(g => g.Key, g => g.Count());

            // Print each unique entry with its duplicate count
            foreach (var entry in duplicatesCount)
            {
                Console.WriteLine($"({entry.Key.x}, {entry.Key.y}): {entry.Value}");
            }
        }
    }
}
