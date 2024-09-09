using DropletsInMotion.Compilers.Models;
using DropletsInMotion.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DropletsInMotion.Controllers
{
    class TemplateHandler
    {
        private List<(string, List<BoardActionDto>)> templates = new List<(string, List<BoardActionDto>)>();
        public Electrode[][] Board { get; set; }

        public TemplateHandler(Electrode[][] board)
        {
            Board = board;
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName ?? "";
            string templatesPath = Path.Combine(projectDirectory, "Assets", "Templates");
            LoadTemplatesFromFiles(templatesPath);
            PrintAllTemplates();
        }

        private void LoadTemplatesFromFiles(string folderPath)
        {
            // Get all txt files in the specified folder
            var templateFiles = Directory.GetFiles(folderPath, "*.txt");

            foreach (var filePath in templateFiles)
            {
                string templateName = Path.GetFileNameWithoutExtension(filePath);
                List<BoardActionDto> boardActions = ParseTemplateFile(filePath);

                if (boardActions != null)
                {
                    templates.Add((templateName, boardActions));
                }
            }
        }

        private List<BoardActionDto> ParseTemplateFile(string filePath)
        {
            var boardActions = new List<BoardActionDto>();
            string[] lines = File.ReadAllLines(filePath);
            decimal timeOffset = 0m;
            int gridSize = lines.Skip(1).First(line => !line.Contains(",") && !string.IsNullOrWhiteSpace(line)).Length; // Determine grid size from the first non-time, non-empty line
            int[] previousState = new int[gridSize * gridSize];

            int rowIndex = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue; // Skip empty lines
                }

                if (line.Contains(","))
                {
                    string[] parts = line.Split(',');
                    Console.WriteLine(parts[0].Trim());
                    Console.WriteLine(decimal.Parse(parts[0].Trim(), CultureInfo.InvariantCulture));
                    timeOffset = decimal.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
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
                        int electrodeIdOffset = (rowIndex - centerRow) * 32 + (colIndex - centerCol);

                        // Ensure indices are within the grid bounds
                        if (rowIndex >= 0 && rowIndex < gridSize && colIndex >= 0 && colIndex < gridSize)
                        {
                            int index = rowIndex * gridSize + colIndex;

                            // Only create actions for changes in state
                            if (action == 1 && previousState[index] == 0)
                            {
                                boardActions.Add(new BoardActionDto(electrodeIdOffset, 1, timeOffset));
                                previousState[index] = 1;
                            }
                            else if (action == 0 && previousState[index] == 1)
                            {
                                boardActions.Add(new BoardActionDto(electrodeIdOffset, 0, timeOffset));
                                previousState[index] = 0;
                            }
                        }
                    }

                    rowIndex++;
                }
            }

            return boardActions;
        }



        public List<BoardActionDto> ApplyTemplate(string templateName, Droplet droplet, decimal time)
        {
            List<BoardActionDto> template = templates.Find(t => t.Item1 == templateName).Item2;
            int relativePosition = Board[droplet.PositionX][droplet.PositionY].Id;
            List<BoardActionDto> finalActionDtos = new List<BoardActionDto>();
            foreach (BoardActionDto boardAction in template)
            {
                BoardActionDto newAction = new BoardActionDto(boardAction.ElectrodeId + relativePosition, boardAction.Action, boardAction.Time + time);
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
    }
}