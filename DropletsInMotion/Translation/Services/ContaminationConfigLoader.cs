using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Infrastructure.Repositories;
using DropletsInMotion.Infrastructure.Services;
using Microsoft.VisualBasic.FileIO;



namespace DropletsInMotion.Translation.Services;

public class ContaminationConfigLoader : IContaminationConfigLoader
{
    private readonly IContaminationRepository _contaminationRepository;
    private readonly IFileService _fileService;
    private readonly IUserService _userService;


    private List<string> ContaminationTableHeaders { get; set; }
    private List<string> MergeTableHeaders { get; set; }

    private List<string> CombinedHeaders { get; set; }

    private string[] ContaminationTableContent { get; set; }
    private string[] MergeTableContent { get; set; }

    private char ContaminationDelimiter { get; set; }
    private char MergeDelimiter { get; set; }

    public ContaminationConfigLoader(IContaminationRepository contaminationRepository, IFileService fileService, IUserService userService)
    {
        _contaminationRepository = contaminationRepository;
        _fileService = fileService;
        _userService = userService;
    }

    public void Load()
    {
        Initialize();

        LoadContaminationTable();
        LoadMergeTable();
    }


    private void Initialize()
    {
        if (_userService.ContaminationTablePath == null)
        {
            _contaminationRepository.ContaminationTable = new List<List<bool>>();
            return;
        }

        if (_userService.MergeTablePath == null)
        {
            _contaminationRepository.MergeTable = new List<List<int>>();
            return;
        }

        ContaminationTableContent = _fileService.ReadFileFromPath(_userService.ContaminationTablePath).Split("\n");
        MergeTableContent = _fileService.ReadFileFromPath(_userService.MergeTablePath).Split("\n");

        var contaminationDelimiter = DetectDelimiter(ContaminationTableContent);
        var mergeDelimiter = DetectDelimiter(MergeTableContent);

        ContaminationDelimiter = contaminationDelimiter;
        MergeDelimiter = mergeDelimiter;

        var contaminationHeaders = GetHeader(ContaminationTableContent, contaminationDelimiter);
        var mergeHeaders = GetHeader(MergeTableContent, mergeDelimiter);
        ContaminationTableHeaders = contaminationHeaders;
        MergeTableHeaders = mergeHeaders;

        var combinedHeaders = contaminationHeaders.Union(mergeHeaders).ToList();
        CombinedHeaders = combinedHeaders;

        // Add the base values to the substance table
        Console.WriteLine("Headers");
        foreach (var s in combinedHeaders)
        {
            _contaminationRepository.SubstanceTable.Add((s, true));
            Console.Write(s + " ");
        }
        Console.WriteLine();
    }


    private char DetectDelimiter(string[] lines)
    {
        string firstLine = lines.First();

        // Find delimiter
        int commaCount = firstLine.Split(',').Length - 1;
        int semicolonCount = firstLine.Split(';').Length - 1;

        // Determine the delimiter
        if (semicolonCount > commaCount)
        {
            return ';';
        }
        else if (commaCount > semicolonCount)
        {
            return ',';
        }
        else
        {
            throw new Exception("Unable to determine the delimiter. Counts are equal.");
        }
    }

    private List<string> GetHeader(string[] lines, char delimiter)
    {
        var headerString = lines.First();
        var header = headerString.Split(delimiter).ToList();

        header.RemoveAll(s => string.IsNullOrWhiteSpace(s.Trim()));
        header = header.Select(s => s.Trim()).ToList();
        return header;
    }

    private void LoadContaminationTable()
    {
        // If no file was specified, we just give it an empty list
        if (_userService.ContaminationTablePath == null && !CombinedHeaders.Any())
        {
            _contaminationRepository.ContaminationTable = new List<List<bool>>();
            return;
        }

        int defaultValue = -1;

        int[,] cTable = new int[CombinedHeaders.Count, CombinedHeaders.Count];
        for (int i = 0; i < cTable.GetLength(0); i++)
        {
            for (int j = 0; j < cTable.GetLength(1); j++)
            {
                cTable[i, j] = defaultValue;
            }
        }

        var linesWithoutHeader = ContaminationTableContent.Skip(1).ToList();

        for (int i = 0; i < linesWithoutHeader.Count; i++)
        {
            var line = linesWithoutHeader[i];
            if (line.Trim() == "") continue;

            var parts = line.Split(ContaminationDelimiter);
            var rowIdentifier = parts.First();

            var rowIndex = CombinedHeaders.IndexOf(rowIdentifier);

            var partsWithoutIdentifier = parts.Skip(1).ToList();
            if (partsWithoutIdentifier == null) throw new Exception("There were no parts in the contamination table");


            for (int j = 0; j < partsWithoutIdentifier.Count(); j++){
                var part = partsWithoutIdentifier[j];
                var trimmedPart = part.Trim();

                var currentIdentifier = ContaminationTableHeaders[j];
                var columnIndex = CombinedHeaders.IndexOf(currentIdentifier);

                if (rowIndex == columnIndex && trimmedPart == "")
                {
                    cTable[rowIndex, columnIndex] = 0;
                }
                else if (trimmedPart == "" || trimmedPart != "0")
                {
                    cTable[rowIndex, columnIndex] = 1;
                }
                else
                {
                    cTable[rowIndex, columnIndex] = 0;
                }

            }

        }


        // Update unknown values
        for (int i = 0; i < cTable.GetLength(0); i++)
        {
            for (int j = 0; j < cTable.GetLength(1); j++)
            {
                if (cTable[i, j] == defaultValue)
                {
                    if (i == j)
                    {
                        cTable[i, j] = 0;
                    }
                    else
                    {
                        cTable[i, j] = 1;
                    }
                }
            }
        }

        _contaminationRepository.ContaminationTable = Enumerable.Range(0, cTable.GetLength(0))
            .Select(i => Enumerable.Range(0, cTable.GetLength(1))
                .Select(j => cTable[i, j] != 0).ToList())
            .ToList();
        
        
        Console.WriteLine("Contamination table");
        for (int i = 0; i < cTable.GetLength(0); i++)
        {
            for (int j = 0; j < cTable.GetLength(1); j++)
            {
                Console.Write(cTable[i, j] + " ");
            }
            Console.WriteLine();
        }

    }


    private void LoadMergeTable()
    {
        Console.WriteLine("MERGE TABLE");

        if (_userService.MergeTablePath == null && !CombinedHeaders.Any())
        {
            _contaminationRepository.MergeTable = new List<List<int>>();
            return;
        }

        int defaultValue = -1;

        int[,] mTable = new int[CombinedHeaders.Count, CombinedHeaders.Count];
        for (int i = 0; i < mTable.GetLength(0); i++)
        {
            for (int j = 0; j < mTable.GetLength(1); j++)
            {
                mTable[i, j] = defaultValue;
            }
        }

        var linesWithoutHeader = MergeTableContent.Skip(1).ToList();

        List<string> addedIdentifiers = new List<string>();

        for (int i = 0; i < linesWithoutHeader.Count; i++)
        {
            var line = linesWithoutHeader[i];
            if (line.Trim() == "") continue;

            var parts = line.Split(MergeDelimiter);
            var rowIdentifier = parts.First();

            var rowIndex = CombinedHeaders.IndexOf(rowIdentifier);

            var partsWithoutIdentifier = parts.Skip(1).ToList();
            if (partsWithoutIdentifier == null) throw new Exception("There were no parts in the contamination table");


            

            for (int j = 0; j < partsWithoutIdentifier.Count(); j++)
            {
                var part = partsWithoutIdentifier[j];
                var trimmedPart = part.Trim();

                var columnIdentifier = MergeTableHeaders[j];
                var columnIndex = CombinedHeaders.IndexOf(columnIdentifier);

                if (rowIndex == columnIndex && trimmedPart == "")
                {
                    mTable[rowIndex, columnIndex] = rowIndex;
                }
                else if (trimmedPart == "")
                {
                    if (addedIdentifiers.Contains(rowIdentifier + "_" + columnIdentifier) || addedIdentifiers.Contains(columnIdentifier + "_" + rowIdentifier)) continue;

                    int index = _contaminationRepository.SubstanceTable.Count;
                    _contaminationRepository.SubstanceTable.Add((rowIdentifier + "_" + columnIdentifier, false));

                    addedIdentifiers.Add(rowIdentifier + "_" + columnIdentifier);
                    addedIdentifiers.Add(columnIdentifier + "_"  + rowIdentifier);

                    mTable[rowIndex, columnIndex] = index;
                    mTable[columnIndex, rowIndex] = index;
                }
                else
                {
                    var index = CombinedHeaders.IndexOf(part);

                    if (index == -1)
                    {
                        throw new Exception($"The identifier \"{part}\" from the merge table was not part of the headers!");
                    }

                    mTable[rowIndex, columnIndex] = index;
                }
            }
        }


        // Update unknown values
        for (int i = 0; i < mTable.GetLength(0); i++)
        {
            for (int j = 0; j < mTable.GetLength(1); j++)
            {
                if (mTable[i, j] == defaultValue)
                {
                    if (i == j)
                    {
                        mTable[i, j] = i;
                    }
                    else
                    {
                        mTable[i, j] = -1;

                        var rowIdentifier = CombinedHeaders[i];
                        var columnIdentifier = CombinedHeaders[j];

                        if (addedIdentifiers.Contains(rowIdentifier + "_" + columnIdentifier) || addedIdentifiers.Contains(columnIdentifier + "_" + rowIdentifier)) continue;


                        int index = _contaminationRepository.SubstanceTable.Count;
                        _contaminationRepository.SubstanceTable.Add((rowIdentifier + "_" + columnIdentifier, false));

                        addedIdentifiers.Add(rowIdentifier + "_" + columnIdentifier);
                        addedIdentifiers.Add(columnIdentifier + "_" + rowIdentifier);

                        mTable[i, j] = index;
                        mTable[j, i] = index;


                    }
                }
            }
        }

       
        Console.WriteLine("Merge table");
        for (int i = 0; i < mTable.GetLength(0); i++)
        {
            for (int j = 0; j < mTable.GetLength(1); j++)
            {
                Console.Write(mTable[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
}

