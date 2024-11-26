namespace DropletsInMotion.Infrastructure.Repositories
{
    public class ContaminationRepository : IContaminationRepository
    {
        public List<List<bool>> ContaminationTable { get; set; }
        public List<List<int>> MergeTable { get; set; }

        public List<(string, (int contTableFrom, int contTableTo, int mergeTableRow, int mergeTableColumn))> SubstanceTable { get; set; }
        
    }
}
