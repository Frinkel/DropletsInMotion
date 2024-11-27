namespace DropletsInMotion.Infrastructure.Repositories;

public interface IContaminationRepository
{
    List<List<bool>> ContaminationTable { get; set; }
    List<List<int>> MergeTable { get; set; }
    List<(string, bool)> SubstanceTable { get; set; }
    Dictionary<(int, int), int> MergeSubstanceTable { get; set; }

    public int GetMergeSubstanceValue(int a, int b);
}