using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Domain;
using System.Globalization;

namespace DropletsInMotion.Controllers
{
    public class TemplateHandler
    {
        public List<(string, List<BoardAction>)> templates { get; private set; } = new List<(string, List<BoardAction>)>();
        public Electrode[][] Board { get; set; }

        public TemplateHandler(Electrode[][] board)
        {
            Board = board;
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
            string templatesPath = Path.Combine(projectDirectory, "Assets", "Templates");
            LoadTemplatesFromFiles(templatesPath);
            //PrintAllTemplates();
        }

        public void LoadTemplatesFromFiles(string folderPath)
        {
            // Get all txt files in the specified folder
            var templateFiles = Directory.GetFiles(folderPath, "*.txt");

            foreach (var filePath in templateFiles)
            {
                string templateName = Path.GetFileNameWithoutExtension(filePath);
                List<BoardAction> boardActions = ParseTemplateFile(filePath);

                if (boardActions != null)
                {
                    templates.Add((templateName, boardActions));
                }
            }
        }

        private List<BoardAction> ParseTemplateFile(string filePath)
        {
            var boardActions = new List<BoardAction>();
            string[] lines = File.ReadAllLines(filePath);
            double timeOffset = 0;
            int gridSize = lines.Skip(1).First(line => !line.Contains(",") && !string.IsNullOrWhiteSpace(line)).Length; // Determine grid size from the first non-time, non-empty line
            int[] previousState = new int[gridSize * gridSize];

            int rowIndex = 0;
            int boardWidth = Board.Length;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue; // Skip empty lines
                }

                if (line.Contains(","))
                {
                    string[] parts = line.Split(',');
                    timeOffset = double.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                    rowIndex = 0; // Reset row index for the next grid
                }
                else if (line.Contains(";"))
                {
                    // End of a grid block, do nothing
                }
                else
                {
                    // Parse the action grid row by row
                    char[] chars = line.Trim().ToCharArray();

                    for (int colIndex = 0; colIndex < chars.Length; colIndex++)
                    {
                        int action = chars[colIndex] - '0';

                        // Calculate electrodeIdOffset relative to the center of the grid
                        int centerRow = gridSize / 2;
                        int centerCol = gridSize / 2;
                        int electrodeIdOffset = (rowIndex - centerRow) * boardWidth + (colIndex - centerCol);

                        // Ensure indices are within the grid bounds
                        if (rowIndex >= 0 && rowIndex < gridSize && colIndex >= 0 && colIndex < gridSize)
                        {
                            int index = rowIndex * gridSize + colIndex;

                            // Only create actions for changes in state
                            if (action == 1 && previousState[index] == 0)
                            {
                                boardActions.Add(new BoardAction(electrodeIdOffset, 1, timeOffset));
                                previousState[index] = 1;
                            }
                            else if (action == 0 && previousState[index] == 1)
                            {
                                boardActions.Add(new BoardAction(electrodeIdOffset, 0, timeOffset));
                                previousState[index] = 0;
                            }
                        }
                    }

                    rowIndex++;
                }
            }

            return boardActions;
        }

        public List<BoardAction> ApplyTemplate(string templateName, Droplet droplet, double time)
        {
            List<BoardAction> template = templates.Find(t => t.Item1 == templateName).Item2;
            int relativePosition = Board[droplet.PositionX][droplet.PositionY].Id;
            List<BoardAction> finalActionDtos = new List<BoardAction>();
            foreach (BoardAction boardAction in template)
            {
                BoardAction newAction = new BoardAction(boardAction.ElectrodeId + relativePosition, boardAction.Action, boardAction.Time + time);
                finalActionDtos.Add(newAction);
            }
            return finalActionDtos;
        }


        // Method to print all templates
        public void PrintAllTemplates()
        {
            foreach (var template in templates)
            {
                Console.WriteLine($"Template Name: {template.Item1}");
                Console.WriteLine("Actions:");
                foreach (var action in template.Item2)
                {
                    Console.WriteLine($"  ElectrodeId: {action.ElectrodeId}, Action: {action.Action}, Time: {action.Time}");
                }
                Console.WriteLine();
            }
        }

        public List<BoardAction> SpinTemplate(List<BoardAction> template)
        {   
            List<BoardAction> newTemplate = new List<BoardAction>();
            int boardWidth = Board.Length;
            foreach (var action in template)
            {
                BoardAction newAction = new BoardAction(action.ElectrodeId, action.Action, action.Time);
                if (Math.Abs(action.ElectrodeId) < boardWidth)
                {
                    newAction.ElectrodeId *= boardWidth;
                }
                else
                {
                    newAction.ElectrodeId /= boardWidth;
                }
                newTemplate.Add(newAction);
            }
            return newTemplate;
        }
    }
}