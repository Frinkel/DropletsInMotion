namespace DropletsInMotion.Infrastructure.Repositories
{
    public class ContaminationRepository : IContaminationRepository
    {
        public List<List<bool>> ContaminationTable { get; set; } = new List<List<bool>>();
        public List<List<int>> MergeTable { get; set; } = new List<List<int>>();

        public List<(string, bool)> SubstanceTable { get; set; } = new List<(string, bool)>();
        
        public Dictionary<(int, int), int> MergeSubstanceTable { get; set; } = new Dictionary<(int, int), int>();

        public int GetMergeSubstanceValue(int a, int b)
        {
            var key = a < b ? (a, b) : (b, a);

            if (MergeSubstanceTable.TryGetValue(key, out int value))
            {
                return value;
            }

            return -1;
        }


    }



}
