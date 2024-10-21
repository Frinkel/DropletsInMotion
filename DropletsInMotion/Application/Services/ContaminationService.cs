using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System.Runtime.Intrinsics.X86;

namespace DropletsInMotion.Application.Services
{
    public class ContaminationService : IContaminationService
    {
        private readonly IConfiguration _configuration;
        private readonly IPlatformRepository _platformRepository;

        public ContaminationService(IConfiguration configuration, IPlatformRepository platformRepository)
        {
            _configuration = configuration;
            _platformRepository = platformRepository;
        }

        public void ApplyIfInBoundsWithContamination(byte[,] contaminationMap, int xPos, int yPos, byte substanceId)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                byte oldValue = contaminationMap[xPos, yPos];
                byte newValue = (byte)(oldValue == 0 || oldValue == substanceId ? substanceId : 255);

                contaminationMap[xPos, yPos] = newValue;
            }
        }

        public void ApplyIfInBounds(byte[,] contaminationMap, int xPos, int yPos, byte substanceId)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                contaminationMap[xPos, yPos] = substanceId;
            }
        }


        public byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap)
        {
            //return ApplyContaminationWithSize(agent, contaminationMap);
            // for disabeling contamination
            //if (!_configuration.GetValue<bool>("Development:Contaminations"))
            //{
            //    return contaminationMap;
            //}
            var x = agent.PositionX;
            var y = agent.PositionY;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);


            // Apply contamination to the agent's position and its 8 neighbors
            ApplyIfInBoundsWithContamination(contaminationMap, x, y, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x + 1, y, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x - 1, y, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x, y + 1, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x, y - 1, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x + 1, y + 1, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x + 1, y - 1, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x - 1, y + 1, agent.SubstanceId);
            ApplyIfInBoundsWithContamination(contaminationMap, x - 1, y - 1, agent.SubstanceId);

            return contaminationMap;
        }


        public void ApplyIfInBoundsMerge(byte[,] contaminationMap, int xPos, int yPos, int finalX, int finalY, byte substanceIdInput1, byte substanceIdInput2, byte substanceIdOutput)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                byte currentSubstanceId = contaminationMap[xPos, yPos];

                //byte newValue = (byte)(currentSubstanceId == 0 || currentSubstanceId == substanceIdInput1 || currentSubstanceId == substanceIdInput2 ? substanceIdOutput : 255);

                //byte newValue2 = (byte)(currentSubstanceId == 0 && currentSubstanceId == substanceIdInput1 || currentSubstanceId == substanceIdInput2 ? substanceIdOutput : 255);
                byte newValue = currentSubstanceId switch
                {
                    byte b when b == substanceIdInput1 => substanceIdInput1,
                    byte b when b == substanceIdInput2 => substanceIdInput2,
                    0 => substanceIdOutput,
                    255 => 255,
                    _ => 255
                };


                contaminationMap[xPos, yPos] = newValue;
            }



        }

        public byte[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePosition, byte[,] contaminationMap)
        {

            var contaminationCoordinates = mergePosition.Template.ContaminationPositions;
            int mergeX = mergePosition.X1;
            int mergeY = mergePosition.Y1;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);


            // Apply contamination positions
            foreach (var pos in contaminationCoordinates)
            {
                
                Console.WriteLine($"pos {pos.x} {pos.y}");

                // Calculate the coordinates relative to the center
                int xPos = pos.x + mergeX;
                int yPos = pos.y + mergeY;

                if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
                {
                    byte currentSubstanceId = contaminationMap[xPos, yPos];

                    byte newValue = currentSubstanceId switch
                    {
                        byte b when b == inputAgent1.SubstanceId => inputAgent1.SubstanceId,
                        byte b when b == inputAgent2.SubstanceId => inputAgent2.SubstanceId,
                        0 => outputAgent.SubstanceId,
                        255 => 255,
                        _ => 255
                    };


                    contaminationMap[xPos, yPos] = newValue;
                }
            }

            
            // Apply contamination for the new agent
            //int topContamination




            //ApplyContaminationWithSize(outputAgent, contaminationMap);

            return contaminationMap;
        }

        public byte[,] ApplyContaminationWithSize(Agent agent, byte[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int size = 1;
            //TODO skal de 1200 være en variabel? eller global i platformRepository
            if (agent.Volume > 1200)
            {
                size = 2;
            }
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);


            // Loop over the area of the droplet, size x size
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    // Apply contamination to the droplet area
                    ApplyIfInBoundsWithContamination(contaminationMap, x + i, y + j, agent.SubstanceId);
                }
            }

            // Apply contamination to the neighbors around the droplet area
            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
                    if (i >= 0 && i < size && j >= 0 && j < size)
                    {
                        // Skip the internal droplet cells that are already contaminated
                        continue;
                    }

                    // Apply contamination to the neighboring cells
                    ApplyIfInBoundsWithContamination(contaminationMap, x + i, y + j, agent.SubstanceId);
                }
            }

            return contaminationMap;
        }



        // TEMP FUNCTIONS
        public void PrintContaminationState(byte[,] contaminationMap)
        {
            // Determine the maximum number of digits for proper alignment
            int maxDigits = 3;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            for (int j = 0; j < colCount; j++)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    byte value = contaminationMap[i, j];

                    // Set the color based on the value using a hash function
                    SetColorForValue(value);

                    // Print each ElectrodeId with a fixed width
                    Console.Write(value.ToString().PadLeft(maxDigits) + " ");

                    // Reset color after printing
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        public bool IsAreaContaminated(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            int endX = Math.Min(rowCount - 1, startX + width - 1);
            int endY = Math.Min(colCount - 1, startY + height - 1);


            // Iterate over the rectangular area and check for contamination
            for (int x = Math.Max(0, startX); x <= endX; x++)
            {
                for (int y = Math.Max(0, startY); y <= endY; y++)
                {
                    // Check if the cell is contaminated
                    if (contaminationMap[x, y] != 0 && contaminationMap[x, y] != substanceId)
                    {
                        return true; // Contamination detected
                    }
                }
            }

            return false; // No contamination found
        }

        public void UpdateContaminationArea(byte[,] contaminationMap, byte substanceId, int startX, int startY, int width, int height)
        {
            // for disabeling contamination
            //if (!_configuration.GetValue<bool>("Development:Contaminations"))
            //{
            //    return;
            //}

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            int endX = Math.Min(rowCount - 1, startX + width);
            int endY = Math.Min(colCount - 1, startY + height);
            // Iterate over the rectangular area and update the contamination map
            for (int x = Math.Max(0, startX); x <= endX; x++)
            {
                for (int y = Math.Max(0, startY); y <= endY; y++)
                {
                    byte oldValue = contaminationMap[x, y];
                    byte newValue = (byte)(oldValue == 0 || oldValue == substanceId ? substanceId : 255);
                    contaminationMap[x, y] = newValue;
                }
            }
        }

        private static void SetColorForValue(byte value)
        {
            // Handle 0 as a special case
            if (value == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                // Get a unique color for each value using a hash function
                var rgbColor = GetRGBFromHash(value);

                // Convert RGB to closest ConsoleColor
                var consoleColor = GetConsoleColorFromRGB(rgbColor.R, rgbColor.G, rgbColor.B);
                Console.BackgroundColor = consoleColor;
            }
        }

        // Method to generate an RGB color from a hash of the value
        private static (byte R, byte G, byte B) GetRGBFromHash(byte value)
        {
            // Hash the value (using a simple hash function for demonstration)
            int hash = (int)(value * 2654435761); // A simple, fast hash function

            // Extract RGB components from the hash
            byte r = (byte)(hash >> 16 & 0xFF);
            byte g = (byte)(hash >> 8 & 0xFF);
            byte b = (byte)(hash & 0xFF);

            return (r, g, b);
        }

        // Convert RGB to a ConsoleColor (approximation due to limited colors in Console)
        private static ConsoleColor GetConsoleColorFromRGB(byte r, byte g, byte b)
        {
            // Calculate the brightness level as an average of the RGB components
            int brightness = (r + g + b) / 3;

            // Check which ranges each component falls into to decide on the color
            if (brightness < 64)
            {
                if (r > g && r > b) return ConsoleColor.DarkRed;
                if (g > r && g > b) return ConsoleColor.DarkGreen;
                if (b > r && b > g) return ConsoleColor.DarkBlue;
                return ConsoleColor.Black;
            }
            else if (brightness < 128)
            {
                if (r > g && r > b) return ConsoleColor.Red;
                if (g > r && g > b) return ConsoleColor.Green;
                if (b > r && b > g) return ConsoleColor.Blue;
                if (r == g && r > b) return ConsoleColor.Yellow;
                if (r == b && r > g) return ConsoleColor.Magenta;
                if (g == b && g > r) return ConsoleColor.Cyan;
                return ConsoleColor.DarkGray;
            }
            else if (brightness < 192)
            {
                if (r > g && r > b) return ConsoleColor.DarkYellow;
                if (g > r && g > b) return ConsoleColor.DarkGreen;
                if (b > r && b > g) return ConsoleColor.DarkBlue;
                if (r == g && r > b) return ConsoleColor.Yellow;
                if (r == b && r > g) return ConsoleColor.Magenta;
                if (g == b && g > r) return ConsoleColor.Cyan;
                return ConsoleColor.Gray;
            }
            else
            {
                if (r > g && r > b) return ConsoleColor.Red;
                if (g > r && g > b) return ConsoleColor.Green;
                if (b > r && b > g) return ConsoleColor.Blue;
                if (r == g && r > b) return ConsoleColor.Yellow;
                if (r == b && r > g) return ConsoleColor.Magenta;
                if (g == b && g > r) return ConsoleColor.Cyan;
                return ConsoleColor.White;
            }
        }

        public void CopyContaminationMap(byte[,] source, byte[,] destination)
        {
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    destination[i, j] = source[i, j];
                }
            }
        }

    }


}
