using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using static System.Reflection.Metadata.BlobBuilder;

namespace DropletsInMotion.Infrastructure.Repositories;
public class TemplateRepository : ITemplateRepository
{
    private class Block
    {
        public string[] Template { get; set; }
        public double TimeOffset { get; set; }

        public Block(string[] template, double timeOffset)
        {
            Template = template;
            TimeOffset = timeOffset;
        }
    }




    private readonly IPlatformRepository _platformRepository;

    public List<SplitTemplate> SplitTemplates { get; private set; } = new List<SplitTemplate>();

    public List<RavelTemplate> RavelTemplates { get; private set; } = new List<RavelTemplate>();

    public List<UnravelTemplate> UnravelTemplates { get; private set; } = new List<UnravelTemplate>();


    private List<Block> _blocks = new();
    private static readonly string[] Separator = ["\r\n", "\r", "\n"];

    public TemplateRepository(IPlatformRepository platformRepository)
    {
        _platformRepository = platformRepository;
        Initialize();
    }

    public void Initialize()
    {
        SplitTemplates = new List<SplitTemplate>();
    }

    public void AddSplit(SplitTemplate splitTemplate, string template)
    {
        // Find all the blocks and actions
        splitTemplate.Actions = ParseTemplateFile(template);

        Block firstBlock = _blocks.First();
        var initialPositions = FindClusters(firstBlock.Template);

        // Validate initial positions
        if (initialPositions.Count != 1)
        {
            throw new InvalidOperationException($"The split template \"{splitTemplate.Name}\" did not start with exactly 1 droplet!");
        }
        // Add the initial positions
        initialPositions.ForEach(pos => splitTemplate.InitialPositions.Add(pos.id.ToString(), (pos.x, pos.y)));

        // Find the end positions
        Block finalBlock = _blocks.Last();
        var finalPositions = FindClusters(finalBlock.Template);

        // Validate final positions
        if (finalPositions.Count != 2)
        {
            throw new InvalidOperationException($"The split template \"{splitTemplate.Name}\" did not result in exactly 2 end droplets!");
        }

        // Add the final positions
        finalPositions.ForEach(pos => splitTemplate.FinalPositions.Add(pos.id.ToString(), (pos.x, pos.y)));


        foreach (var idPos in splitTemplate.FinalPositions)
        {
            var id = idPos.Key;
            if (!splitTemplate.RatioRelation.ContainsKey(id))
            {
                throw new Exception(
                    $"Ratio relation for id: {id} was not found in split template \"{splitTemplate.Name}\"");
            }
        }

        SplitTemplates.Add(splitTemplate);

        Console.WriteLine(splitTemplate);
    }

    public void AddRavel(RavelTemplate ravelTemplate, string template)
    {
        // Find all the blocks and actions
        ravelTemplate.Actions = ParseTemplateFile(template);
        ;
        ravelTemplate.Duration = _blocks.Last().TimeOffset;

        RavelTemplates.Add(ravelTemplate);


        Console.WriteLine(ravelTemplate);
    }

    public void AddUnravel(UnravelTemplate unravelTemplate, string template)
    {
        // Find all the blocks and actions
        unravelTemplate.Actions = ParseTemplateFile(template);

        unravelTemplate.Duration = _blocks.Last().TimeOffset;

        UnravelTemplates.Add(unravelTemplate);


        Console.WriteLine(unravelTemplate);
    }




    private List<BoardAction> ParseTemplateFile(string template)
    {
        _blocks.Clear();
        var boardActions = new List<BoardAction>();
        int boardWidth = _platformRepository.Board.Length;
        string[] stringBlocks = template.Split(";");
        int gridSize = stringBlocks.First().Split(",")[1].Split(Separator, StringSplitOptions.None).Skip(1).First().Length;
        string[] previousBlock = new string[gridSize];


        foreach (var stringBlock in stringBlocks)
        {
            var timeOffset = double.Parse(stringBlock.Split(",")[0].Trim(), CultureInfo.InvariantCulture);
            string blockTemplate = stringBlock.Split(",")[1];
            var lines = blockTemplate.Trim().Split(Separator, StringSplitOptions.None);

            Block block = new Block(lines, timeOffset);
            _blocks.Add(block);
        }

        foreach (var block in _blocks)
        {
            var lines = block.Template;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                //Console.WriteLine(line);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }


                char[] prevChars = (previousBlock != null) && (previousBlock?[i] != null) ? previousBlock[i].Trim().ToCharArray() : Enumerable.Repeat('0', gridSize).ToArray();
                char[] chars = line.Trim().ToCharArray();

                for (int colIndex = 0; colIndex < chars.Length; colIndex++)
                {
                    int action = chars[colIndex] - '0';
                    int prevAction = prevChars[colIndex] - '0';

                    int centerRow = gridSize / 2;
                    int centerCol = gridSize / 2;
                    int electrodeIdOffset = (i - centerRow) * boardWidth + (colIndex - centerCol);


                    if (action != 0 && prevAction == 0)
                    {
                        boardActions.Add(new BoardAction(electrodeIdOffset, 1, block.TimeOffset));
                    }
                    else if (action == 0 && prevAction != 0)
                    {
                        boardActions.Add(new BoardAction(electrodeIdOffset, 0, block.TimeOffset));
                    }
                }
            }

            previousBlock = lines;
        }
        boardActions = boardActions.OrderBy(b => b.Time).ToList();

        return boardActions;
    }

    // Method to find clusters and return their top-left positions
    public List<(int id, int x, int y)> FindClusters(string[] block)
    {
        int rows = block.Length;
        int cols = block[0].Length;
        int[,] grid = new int[rows, cols];

        // Parse the block into a 2D array
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                grid[i, j] = block[i][j] - '0';  // Convert '0'/'1' characters to integers
            }
        }

        bool[,] visited = new bool[rows, cols];
        List<(int id, int row, int col)> topLeftPositions = new List<(int id, int row, int col)>();

        // Direction vectors for navigating neighbors (up, down, left, right)
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        // Method for performing DFS or flood-fill to find a cluster
        void FloodFill(int r, int c, ref int minRow, ref int minCol)
        {
            Stack<(int r, int c)> stack = new Stack<(int r, int c)>();
            stack.Push((r, c));
            visited[r, c] = true;

            while (stack.Count > 0)
            {
                var (curR, curC) = stack.Pop();

                // Update top-left-most position
                if (curR < minRow || (curR == minRow && curC < minCol))
                {
                    minRow = curR;
                    minCol = curC;
                }

                // Visit all neighbors
                for (int i = 0; i < 4; i++)
                {
                    int newRow = curR + dRow[i];
                    int newCol = curC + dCol[i];

                    if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols &&
                        !visited[newRow, newCol] && grid[newRow, newCol] != 0)
                    {
                        visited[newRow, newCol] = true;
                        stack.Push((newRow, newCol));
                    }
                }
            }
        }

        // Iterate over the grid to find clusters
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != 0 && !visited[r, c])
                {
                    // Found a new cluster, start flood-fill
                    int minRow = r, minCol = c;
                    FloodFill(r, c, ref minRow, ref minCol);
                    
                    int gridSize = block.First().Length;
                    int centerRow = gridSize / 2;
                    int centerCol = gridSize / 2;

                    topLeftPositions.Add((grid[r, c], minCol - centerCol, minRow - centerRow));
                }
            }
        }

        return topLeftPositions;
    }
}
