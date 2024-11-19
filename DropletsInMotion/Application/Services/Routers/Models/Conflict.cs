using System.Runtime.InteropServices;
using DropletsInMotion.Application.Models;

namespace DropletsInMotion.Application.Services.Routers.Models
{
    public class Conflict
    {
        public readonly string Name;
        //public readonly List<Agent> Agents;
        //public readonly Agent Agent2;
        //public readonly (int x, int y) Position;
        public readonly int? Time;

        public Dictionary<string, (int x, int y)> Conflicts;

        public Conflict(string name, Dictionary<string, (int x, int y)> conflicts, int? time = null)
        {
            Name = name;
            Conflicts = conflicts;
            //Agents = agents;
            //Agent1 = agent1;
            //Agent2 = agent2;
            Time = time;
            //Position = position;
        }

    }
}
