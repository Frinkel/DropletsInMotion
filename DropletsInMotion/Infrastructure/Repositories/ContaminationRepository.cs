namespace DropletsInMotion.Infrastructure.Repositories
{
    public class ContaminationRepository : IContaminationRepository
    {
        public List<List<bool>> ContaminationTable { get; set; }
        public List<List<int>> MergeTable { get; set; }

        public List<(string, bool)> SubstanceTable { get; set; } = new List<(string, bool)>();
        
    }
}
