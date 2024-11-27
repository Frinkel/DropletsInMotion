﻿namespace DropletsInMotion.Infrastructure.Repositories;

public interface IContaminationRepository
{
    List<List<bool>> ContaminationTable { get; set; }
    List<List<int>> MergeTable { get; set; }
    List<(string, (int contTableFrom, int contTableTo, int mergeTableRow, int mergeTableColumn) )> SubstanceTable { get; set; }
    public Dictionary<(int, int), int> MergeSubstanceTable { get; set; }
    int GetMergeSubstanceValue(int a, int b);
}