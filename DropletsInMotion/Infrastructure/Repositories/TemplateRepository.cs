using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DropletsInMotion.Application.Execution.Models;
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

    public List<MergeTemplate> MergeTemplates { get; private set; } = new List<MergeTemplate>();

    public List<RavelTemplate> RavelTemplates { get; private set; } = new List<RavelTemplate>();

    public List<UnravelTemplate> UnravelTemplates { get; private set; } = new List<UnravelTemplate>();

    public List<DeclareTemplate> DeclareTemplates { get; private set; } = new List<DeclareTemplate>();

    public List<GrowTemplate> GrowTemplates { get; private set; } = new List<GrowTemplate>();

    public List<ShrinkTemplate> ShrinkTemplates { get; private set; } = new List<ShrinkTemplate>();


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

        // Find contamination positions
        //splitTemplate.ContaminationPositions.AddRange(FindContaminationCoordinates(template));


        // Find the block positions with ids
        List<Dictionary<string, List<(int x, int y)>>> blockPositions = new List<Dictionary<string, List<(int x, int y)>>>();
        foreach (var block in _blocks)
        {
            blockPositions.Add(GetClusterPositions(block));
        }
        splitTemplate.Blocks = blockPositions;




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

    public void AddMerge(MergeTemplate mergeTemplate, string template)
    {
        // Find all the blocks and actions
        mergeTemplate.Actions = ParseTemplateFile(template);

        // Find the block positions with ids
        List<Dictionary<string, List<(int x, int y)>>> blockPositions = new List<Dictionary<string, List<(int x, int y)>>>();
        foreach (var block in _blocks)
        {
            blockPositions.Add(GetClusterPositions(block));
        }
        mergeTemplate.Blocks = blockPositions;        


        Block firstBlock = _blocks.First();
        var initialPositions = FindClusters(firstBlock.Template);

        // Validate initial positions
        if (initialPositions.Count != 2)
        {
            throw new InvalidOperationException($"The merge template \"{mergeTemplate.Name}\" did not start with exactly 2 droplet!");
        }
        // Add the initial positions
        initialPositions.ForEach(pos => mergeTemplate.InitialPositions.Add(pos.id.ToString(), (pos.x, pos.y)));

        // Find the end positions
        Block finalBlock = _blocks.Last();
        var finalPositions = FindClusters(finalBlock.Template);

        // Validate final positions
        if (finalPositions.Count != 1)
        {
            throw new InvalidOperationException($"The merge template \"{mergeTemplate.Name}\" did not result in exactly 1 end droplets!");
        }

        // Add the final positions
        finalPositions.ForEach(pos => mergeTemplate.FinalPositions.Add(pos.id.ToString(), (pos.x, pos.y)));

        MergeTemplates.Add(mergeTemplate);

        //Console.WriteLine(splitTemplate);
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

    public void AddDeclare(DeclareTemplate declareTemplate, string template)
    {
        // Find all the blocks and actions
        declareTemplate.Actions = ParseTemplateFile(template);

        DeclareTemplates.Add(declareTemplate);

        Console.WriteLine(declareTemplate);
    }

    public void AddGrow(GrowTemplate growTemplate, string template)
    {
        // Find all the blocks and actions
        growTemplate.Actions = ParseTemplateFile(template);

        GrowTemplates.Add(growTemplate);

        Console.WriteLine(growTemplate);
    }

    public void AddShrink(ShrinkTemplate shrinkTemplate, string template)
    {
        // Find all the blocks and actions
        shrinkTemplate.Actions = ParseTemplateFile(template);

        ShrinkTemplates.Add(shrinkTemplate);

        Console.WriteLine(shrinkTemplate);
    }



    private Dictionary<string, List<(int x, int y)>> GetClusterPositions(Block block)
    {
        int rows = block.Template.Length;
        int cols = block.Template[0].Trim().Length;
        int[,] grid = new int[rows, cols];

        // Parse the block into a 2D array
        for (int i = 0; i < rows; i++)
        {
            var line = block.Template[i].Trim();
            for (int j = 0; j < cols; j++)
            {
                grid[i, j] = line[j] - '0';  // Convert '0'/'1' characters to integers
            }
        }

        bool[,] visited = new bool[rows, cols];
        Dictionary<string, List<(int x, int y)>> clusters = new Dictionary<string, List<(int x, int y)>>();

        // Direction vectors for navigating neighbors (up, down, left, right)
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        void FloodFill(int r, int c, int id)
        {
            Stack<(int r, int c)> stack = new Stack<(int r, int c)>();
            stack.Push((r, c));
            visited[r, c] = true;

            string clusterId = id.ToString();
            if (!clusters.ContainsKey(clusterId))
            {
                clusters[clusterId] = new List<(int x, int y)>();
            }

            int centerRow = rows / 2;
            int centerCol = cols / 2;

            // Add initial position to the cluster
            clusters[clusterId].Add((c - centerCol, r - centerRow));

            while (stack.Count > 0)
            {
                var (curR, curC) = stack.Pop();

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
                        clusters[clusterId].Add((newCol - centerCol, newRow - centerRow));
                    }
                }
            }
        }

        // Iterate over the grid to find clusters
        int clusterId = 1;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != 0 && !visited[r, c])
                {
                    FloodFill(r, c, clusterId);
                    clusterId++;
                }
            }
        }

        return clusters;
    }





    private List<BoardAction> ParseTemplateFile(string template)
    {
        _blocks.Clear();
        var boardActions = new List<BoardAction>();
        int boardWidth = _platformRepository.Board.Length;
        string[] stringBlocks = template.Split(";", StringSplitOptions.RemoveEmptyEntries);

        string[] previousBlock = null;
        int blockRows = 0;
        int blockCols = 0;

        // First, parse all the blocks and ensure they have the same size
        foreach (var stringBlock in stringBlocks)
        {
            var parts = stringBlock.Trim().Split(",", 2);
            if (parts.Length < 2)
                continue;

            var timeOffset = double.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            var blockTemplate = parts[1];

            var lines = blockTemplate.Trim().Split(Separator, StringSplitOptions.None);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray(); // Remove empty lines

            if (blockRows == 0 && blockCols == 0)
            {
                // Initialize block size based on the first block
                blockRows = lines.Length;
                blockCols = lines[0].Trim().Length;
            }
            else
            {
                // Validate that the block size matches the initial block size
                if (lines.Length != blockRows || lines.Any(line => line.Trim().Length != blockCols))
                {
                    throw new InvalidOperationException("All blocks within a template must have the same size.");
                }
            }

            Block block = new Block(lines, timeOffset);
            _blocks.Add(block);
        }

        foreach (var block in _blocks)
        {
            var lines = block.Template;
            int centerRow = blockRows / 2;
            int centerCol = blockCols / 2;

            for (int i = 0; i < blockRows; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                char[] chars = line.ToCharArray();
                char[] prevChars = previousBlock != null ? previousBlock[i].Trim().ToCharArray() : new string('0', blockCols).ToCharArray();

                for (int colIndex = 0; colIndex < blockCols; colIndex++)
                {
                    int action = chars[colIndex] - '0';
                    int prevAction = prevChars[colIndex] - '0';

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
            previousBlock = block.Template;
        }

        boardActions = boardActions.OrderBy(b => b.Time).ToList();
        return boardActions;
    }



    // Method to find clusters and return their top-left positions
    public List<(int id, int x, int y)> FindClusters(string[] block)
    {
        int rows = block.Length;
        int cols = block[0].Trim().Length;
        int[,] grid = new int[rows, cols];

        // Parse the block into a 2D array
        for (int i = 0; i < rows; i++)
        {
            var line = block[i].Trim();
            for (int j = 0; j < cols; j++)
            {
                grid[i, j] = line[j] - '0';  // Convert '0'/'1' characters to integers
            }
        }

        bool[,] visited = new bool[rows, cols];
        List<(int id, int row, int col)> topLeftPositions = new List<(int id, int row, int col)>();

        // Direction vectors for navigating neighbors (up, down, left, right)
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

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
                    int minRow = r, minCol = c;
                    FloodFill(r, c, ref minRow, ref minCol);

                    int centerRow = rows / 2;
                    int centerCol = cols / 2;

                    topLeftPositions.Add((grid[r, c], minCol - centerCol, minRow - centerRow));
                }
            }
        }

        return topLeftPositions;
    }


}
