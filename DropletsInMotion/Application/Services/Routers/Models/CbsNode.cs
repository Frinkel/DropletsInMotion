using DropletsInMotion.Application.Models;

namespace DropletsInMotion.Application.Services.Routers.Models
{
    public class CbsNode
    {
        public HashSet<Constraint> Constraints { get; set; }
        public int Score { get; set; }

        public Dictionary<string, State> Routes { get; set; }

        public int Depth { get; set; }

        public CbsNode(Dictionary<string, State> routes, HashSet<Constraint> constraints, int depth)
        {
            Routes = routes;
            Constraints = constraints;
            Score = CalculateScore();
            Depth = depth;
        }

        // Score is the combined length of all routes
        private int CalculateScore()
        {
            int finalScore = 0;

            

            foreach (var kvp in Routes)
            {
                if (kvp.Value == null) return int.MaxValue;

                finalScore += kvp.Value.G;
            }

            return finalScore;
        }

    }
}
