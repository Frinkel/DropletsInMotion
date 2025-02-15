﻿using System.Diagnostics;

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

        private static Process _process;

        public static Process GetProcess()
        {
            var process = Process.GetCurrentProcess();
            _process = process;
            return process;
        }

        public static void PrintMemoryUsage(Process process)
        {
            process.Refresh();
            var memory = process.WorkingSet64 / (1024.0 * 1024.0);
            Console.WriteLine($"Current memory '{process.ProcessName}': {memory - _prevMemory} MB");
            _prevMemory = memory;
        }

        public static void PrintMemoryUsage2()
        {
            _process.Refresh();
            var memory = _process.WorkingSet64 / (1024.0 * 1024.0);
            Console.WriteLine($"Current memory '{_process.ProcessName}': {memory - _prevMemory} MB");
            Console.WriteLine($"Max mem: {_process.PeakWorkingSet64 / (1024.0 * 1024.0)}");
            _prevMemory = memory;
        }


        public static void PrintDuplicateCounts()
        {
            var duplicatesCount = Nodes
                .GroupBy(p => p)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var entry in duplicatesCount)
            {
                Console.WriteLine($"({entry.Key.x}, {entry.Key.y}): {entry.Value}");
            }
        }
    }
}
