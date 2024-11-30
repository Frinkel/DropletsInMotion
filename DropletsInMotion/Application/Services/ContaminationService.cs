using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

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

        //List<int>[,] array = new List<int>[rows, cols];

        public void ApplyIfInBounds(List<int>[,] contaminationMap, int xPos, int yPos, int substanceId)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                var contaminations = contaminationMap[xPos, yPos];
                if (!contaminations.Contains(substanceId))
                {
                    contaminations.Add(substanceId);
                }
            }
        }

        public void ApplyContamination(Agent agent, State state)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            // Apply contamination to the agent's position and its 8 neighbors
            ApplyIfInBounds(state, x, y, agent.SubstanceId);
            ApplyIfInBounds(state, x + 1, y, agent.SubstanceId);
            ApplyIfInBounds(state, x - 1, y, agent.SubstanceId);
            ApplyIfInBounds(state, x, y + 1, agent.SubstanceId);
            ApplyIfInBounds(state, x, y - 1, agent.SubstanceId);
            ApplyIfInBounds(state, x + 1, y + 1, agent.SubstanceId);
            ApplyIfInBounds(state, x + 1, y - 1, agent.SubstanceId);
            ApplyIfInBounds(state, x - 1, y + 1, agent.SubstanceId);
            ApplyIfInBounds(state, x - 1, y - 1, agent.SubstanceId);
        }

        private void ApplyIfInBounds(State state, int x, int y, int substanceId)
        {
            
            var contaminationValues = state.GetContamination(x, y);

            if (!contaminationValues.Contains(substanceId))
            {
                var updatedContaminationValues = new List<int>(contaminationValues)
                {
                    substanceId
                };

                state.SetContamination(x, y, updatedContaminationValues);
            }
            
        }


        public List<int>[,] ApplyContamination(Agent agent, List<int>[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);


            // Apply contamination to the agent's position and its 8 neighbors
            ApplyIfInBounds(contaminationMap, x, y, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x + 1, y, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x - 1, y, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x, y + 1, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x, y - 1, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x + 1, y + 1, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x + 1, y - 1, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x - 1, y + 1, agent.SubstanceId);
            ApplyIfInBounds(contaminationMap, x - 1, y - 1, agent.SubstanceId);

            return contaminationMap;
        }

        public List<int>[,] ApplyContaminationWithSize(Agent agent, List<int>[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int size = agent.GetAgentSize();

            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    ApplyIfInBounds(contaminationMap, x + i, y + j, agent.SubstanceId);
                }
            }

            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
                    if (i >= 0 && i < size && j >= 0 && j < size)
                    {
                        continue;
                    }
                    ApplyIfInBounds(contaminationMap, x + i, y + j, agent.SubstanceId);
                }
            }

            return contaminationMap;
        }

        public bool IsConflicting(List<int>[,] contaminationMap, int xPos, int yPos, int substanceId)
        {
            List<int> contaminationValues = contaminationMap[xPos, yPos];
            if (contaminationValues.Count == 0)
            {
                return false;
            }

            var substanceFromIndex = _contaminationRepository.SubstanceTable[substanceId].Item2.contTableFrom;
            if(substanceFromIndex == -1)
            {
                if(contaminationValues.Count == 0 ||
                   (contaminationValues.Count == 1 && contaminationValues[0] == substanceId))
                {
                    return false;
                }
                return true;
            }


            //foreach (var value in contaminationValues)
            //{
            //    var substanceToIndex = _contaminationRepository.SubstanceTable[value].Item2.contTableFrom;
            //    if (substanceToIndex == -1)
            //    {
            //        return true;
            //    }
            //    bool isConflicting = _contaminationRepository.ContaminationTable[substanceFromIndex][substanceToIndex];
            //    if (isConflicting)
            //    {
            //        return true;
            //    }
            //}

            foreach (var value in contaminationValues)
            {
                var substanceToIndex = _contaminationRepository.SubstanceTable[value].Item2.contTableFrom;
                if (substanceToIndex == -1 ||
                    _contaminationRepository.ContaminationTable[substanceFromIndex][substanceToIndex])
                {
                    return true;
                }
            }

            return false;
        }


        public bool IsConflicting(List<int> contaminationValues, int substanceId)
        {
            if (contaminationValues.Count == 0)
            {
                return false;
            }

            var substanceFromIndex = _contaminationRepository.SubstanceTable[substanceId].Item2.contTableFrom;
            if (substanceFromIndex == -1)
            {
                if (contaminationValues.Count == 0 ||
                    (contaminationValues.Count == 1 && contaminationValues[0] == substanceId))
                {
                    return false;
                }
                return true;
            }

            foreach (var value in contaminationValues)
            {
                var substanceToIndex = _contaminationRepository.SubstanceTable[value].Item2.contTableFrom;
                if (substanceToIndex == -1 ||
                    _contaminationRepository.ContaminationTable[substanceFromIndex][substanceToIndex])
                {
                    return true;
                }
            }

            return false;
        }

        public int GetResultingSubstanceId(List<int>[,] contaminationMap, int substance1, int substance2)
        {
            var substance1MergeIndex = _contaminationRepository.SubstanceTable[substance1].Item2.contTableFrom;
            var substance2MergeIndex = _contaminationRepository.SubstanceTable[substance2].Item2.contTableFrom;
            if (substance2MergeIndex != -1 && substance2MergeIndex != -1)
            {
                return _contaminationRepository.MergeTable[substance1MergeIndex][substance2MergeIndex];
            }

            int mergedSubstanceId = _contaminationRepository.GetMergeSubstanceValue(substance1, substance2);
            if (mergedSubstanceId != -1)
            {
                return mergedSubstanceId;
            }

            string newSubstanceName = _contaminationRepository.SubstanceTable[substance1].Item1 + "_" + _contaminationRepository.SubstanceTable[substance2].Item1;
            _contaminationRepository.SubstanceTable.Add((newSubstanceName, (-1, -1, -1, -1)));

            var newSubstanceId = _contaminationRepository.SubstanceTable.Count - 1;
            _contaminationRepository.MergeSubstanceTable.Add((substance1,substance2), newSubstanceId);

            return newSubstanceId;
        }

        public List<int>[,] CloneContaminationMap(List<int>[,] contaminationMap)
        {
            int rows = contaminationMap.GetLength(0);
            int cols = contaminationMap.GetLength(1);
            var clonedMap = new List<int>[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    clonedMap[i, j] = contaminationMap[i, j] != null
                        ? new List<int>(contaminationMap[i, j])
                        : null;
                }
            }

            return clonedMap;
        }

        public List<int>[,] CreateContaminationMap(int rows, int cols)
        {
            var contaminationMap = new List<int>[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    contaminationMap[i, j] = new List<int>();
                }
            }

            return contaminationMap;
        }

        public int GetSubstanceId(string substanceName)
        {
            if (substanceName == "")
            {
                return GetNewSubstance("");
            }

            var substanceId = _contaminationRepository.SubstanceTable.FindIndex(item => item.Item1 == substanceName);
            if(substanceId == -1)
            {
                return GetNewSubstance(substanceName);
            }

            return substanceId;
        }

        public int GetNewSubstance(string name)
        {
            _contaminationRepository.SubstanceTable.Add((name, (-1, -1, -1, -1)));
            return _contaminationRepository.SubstanceTable.Count - 1;
        }

        public List<int>[,] ReserveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents, List<int>[,] contaminationMap)
        {
            foreach (var command in commands)
            {
                var agent = agents[command.GetInputDroplets().First()];
                Agent reserveAgent = (Agent)agent.Clone();
                reserveAgent.PositionX = ((Move)command).PositionX;
                reserveAgent.PositionY = ((Move)command).PositionY;
                ApplyContaminationWithSize(reserveAgent, contaminationMap);
            }

            return contaminationMap;
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

        public void ApplyIfInBoundsWithLegalSubstanceIds(byte[,] contaminationMap, int xPos, int yPos, byte substanceId, List<byte> legalSubstanceIds)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                byte contaminationValue = contaminationMap[xPos, yPos];
                byte newValue = (byte)(contaminationValue != 255 && legalSubstanceIds.Contains(contaminationValue) ? contaminationValue : 255);
                newValue = contaminationValue == 0 ? substanceId : newValue;
                

                contaminationMap[xPos, yPos] = newValue;
            }
        }


        public void OverrideContaminations(byte[,] contaminationMap, int xPos, int yPos, byte substanceId, List<byte> overrideableSubstanceIds)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                byte contaminationValue = contaminationMap[xPos, yPos];
                byte newValue = (byte)((contaminationValue != 255 && overrideableSubstanceIds.Contains(contaminationValue)) || contaminationValue == 0 ? substanceId : 255);

                contaminationMap[xPos, yPos] = newValue;
            }
        }


        public byte[,] ApplyContamination(Agent agent, byte[,] contaminationMap)
        {
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



        // Apply contamination for a split while taking the template into account
        public byte[,] ApplyContaminationSplit(Agent inputAgent, ScheduledPosition splitPositions, byte[,] contaminationMap)
        {
            foreach (var block in splitPositions.Template.Blocks)
            {
                foreach (var cluster in block)
                {
                    foreach (var pos in cluster.Value)
                    {
                        ApplyIfInBoundsWithContamination(contaminationMap, inputAgent.PositionX + pos.x, inputAgent.PositionY + pos.y, inputAgent.SubstanceId);
                    }
                }
            }
            return contaminationMap;
        }


        // Apply contamination for a merge while taking the template into account
        public byte[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePositions, byte[,] contaminationMap)
        {
            int mergeX = mergePositions.OriginX;
            int mergeY = mergePositions.OriginY;

            int size = outputAgent.GetAgentSize();

            // Overwrite initial area with new agent substance
            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
                    ApplyIfInBounds(contaminationMap, outputAgent.PositionX + i, outputAgent.PositionY + j, outputAgent.SubstanceId);
                }
            }


            //Console.WriteLine("BEFORE");
            //Console.WriteLine($"Merge template {mergePositions.Template.Name}");
            //PrintContaminationState(contaminationMap);
            // Apply contamination based on the templates
            foreach (var block in mergePositions.Template.Blocks)
            {

                foreach (var cluster in block)
                {

                    foreach (var pos in cluster.Value)
                    {

                        var substanceId = cluster.Key switch
                        {
                            var name when name == inputAgent1.DropletName => inputAgent1.SubstanceId,
                            var name when name == inputAgent2.DropletName => inputAgent2.SubstanceId,
                            var name when name == outputAgent.DropletName => outputAgent.SubstanceId,
                            _ => throw new Exception($"No agent mapping with for agent {cluster.Key}")
                        };


                        if (block.Count <= 1) substanceId = outputAgent.SubstanceId;
                        

                        var contaminationPosX = pos.x + mergeX;
                        var contaminationPosY = pos.y + mergeY;

                        // Define the offsets for our padding of the contamination
                        var offsets = new List<(int xOffset, int yOffset)>
                        {
                            (0, 0),   // Original position
                            (1, 0),   // Right
                            (-1, 0),  // Left
                            (0, 1),   // Down
                            (0, -1),  // Up
                            (1, -1),  // Bottom-right diagonal
                            (-1, 1),  // Top-left diagonal
                            (1, 1),   // Top-right diagonal
                            (-1, -1)  // Bottom-left diagonal
                        };

                        if (substanceId == outputAgent.SubstanceId)
                        {
                            var legalSubstances = new List<byte> { inputAgent1.SubstanceId, inputAgent2.SubstanceId, outputAgent.SubstanceId };

                            foreach (var (xOffset, yOffset) in offsets)
                            {
                                OverrideContaminations(contaminationMap, contaminationPosX + xOffset, contaminationPosY + yOffset, substanceId, legalSubstances);
                            }
                        }
                        else
                        {
                            var legalSubstances = new List<byte> { substanceId, outputAgent.SubstanceId };

                            foreach (var (xOffset, yOffset) in offsets)
                            {
                                ApplyIfInBoundsWithLegalSubstanceIds(contaminationMap, contaminationPosX + xOffset, contaminationPosY + yOffset, substanceId, legalSubstances);
                            }
                        }
                    }
                }
            }


            int GetContaminationValue(int x, int y, byte[,] contaminationMap)
            {
                int rowCount = contaminationMap.GetLength(0);
                int colCount = contaminationMap.GetLength(1);

                if (x >= 0 && x < rowCount && y >= 0 && y < colCount)
                {
                    return contaminationMap[x, y];
                }

                return 255;
            }

            mergeX = mergePositions.SingularX;
            mergeY = mergePositions.SingularY;

            // Calculate contamination overlaps
            for (int i = 0; i <= size + 1; i++)
            {
                int x1 = mergeX - 1 + i;
                int y1 = mergeY - 2;
                int cont1 = GetContaminationValue(x1, y1, contaminationMap);
                int canContChange1 = GetContaminationValue(x1, mergeY - 1, contaminationMap);
                ApplyIfInBounds(contaminationMap, x1, mergeY - 1, (cont1 == 0 || cont1 == outputAgent.SubstanceId) && canContChange1 != 255 ? outputAgent.SubstanceId : (byte)255);

                int x2 = mergeX - 1 + i;
                int y2 = mergeY + size + 1;
                int cont2 = GetContaminationValue(x2, y2, contaminationMap);
                int canContChange2 = GetContaminationValue(x2, mergeY + size, contaminationMap);
                ApplyIfInBounds(contaminationMap, x2, mergeY + size, (cont2 == 0 || cont2 == outputAgent.SubstanceId) && canContChange2 != 255 ? outputAgent.SubstanceId : (byte)255);

                int x3 = mergeX - 2;
                int y3 = mergeY - 1 + i;
                int cont3 = GetContaminationValue(x3, y3, contaminationMap);
                int canContChange3 = GetContaminationValue(mergeX - 1, y3, contaminationMap);
                ApplyIfInBounds(contaminationMap, mergeX - 1, y3, (cont3 == 0 || cont3 == outputAgent.SubstanceId) && canContChange3 != 255 ? outputAgent.SubstanceId : (byte)255);

                int x4 = mergeX + size + 1;
                int y4 = mergeY - 1 + i;
                int cont4 = GetContaminationValue(x4, y4, contaminationMap);
                int canContChange4 = GetContaminationValue(mergeX + size, y3, contaminationMap);
                ApplyIfInBounds(contaminationMap, mergeX + size, y4, (cont4 == 0 || cont4 == outputAgent.SubstanceId) && canContChange4 != 255 ? outputAgent.SubstanceId : (byte)255);
            }


            //Console.WriteLine("AFTER");
            //PrintContaminationState(contaminationMap);

            return contaminationMap;
        }

        public byte[,] ReserveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents, byte[,] contaminationMap)
        {
            foreach (var command in commands)
            {
                var agent = agents[command.GetInputDroplets().First()];
                Agent reserveAgent = (Agent) agent.Clone();
                reserveAgent.PositionX = ((Move)command).PositionX;
                reserveAgent.PositionY = ((Move)command).PositionY;
                ApplyContaminationWithSize(reserveAgent, contaminationMap);
            }

            return contaminationMap;
        }


        public byte[,] ApplyContaminationWithSize(Agent agent, byte[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int size = agent.GetAgentSize();

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


        //private int GetAgentSize(Agent agent)
        //{
        //    int size = 1;
        //    if (agent.Volume > _platformRepository.MinSize2x2)
        //    {
        //        size = 2;
        //    }
        //    if (agent.Volume > _platformRepository.MinSize3x3)
        //    {
        //        size = 3;
        //    }

        //    return size;
        //}


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
