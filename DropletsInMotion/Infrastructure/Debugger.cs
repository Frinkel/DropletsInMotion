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
        
        public static List<(int x, int y)> Nodes = new List<(int x, int y)>();

        public static int Permutations { get; set; }

        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static void PrintMemoryUsage()
        {
            long memory = GC.GetTotalMemory(false);
            Console.WriteLine($"Current memory is: {memory / (1024.0 * 1024.0)} mb");
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
