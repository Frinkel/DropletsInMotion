using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Application.Services
{
    public class ContaminationService : IContaminationService
    {
        private readonly IContaminationRepository _contaminationRepository;

        public ContaminationService(IConfiguration configuration, IPlatformRepository platformRepository, IContaminationRepository contaminationRepository)
        {
            _contaminationRepository = contaminationRepository;
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

        private void ApplyIfInBounds(State state, int xPos, int yPos, int substanceId)
        {
            //TODO add length and width as global values
            int rowCount = state.ContaminationMap.GetLength(0);
            int colCount = state.ContaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {

                var contaminationValues = state.GetContamination(xPos, yPos);

                if (!contaminationValues.Contains(substanceId))
                {
                    var updatedContaminationValues = new List<int>(contaminationValues)
                    {
                        substanceId
                    };

                    state.SetContamination(xPos, yPos, updatedContaminationValues);
                }
            }

        }

        public bool IsConflicting(List<int> contaminationValues, int substanceId)
        {
            if (contaminationValues.Count == 0)
            {
                return false;
            }

            var substanceInContaminationTable1 = _contaminationRepository.SubstanceTable[substanceId].Item2;
            if (!substanceInContaminationTable1)
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
                var substanceInContaminationTable2 = _contaminationRepository.SubstanceTable[value].Item2;

                if (!substanceInContaminationTable2 ||
                    _contaminationRepository.ContaminationTable[substanceId][value])
                {
                    return true;
                }
            }

            return false;
        }


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

        public void RemoveIfInBounds(List<int>[,] contaminationMap, int xPos, int yPos, int substanceId)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            if (xPos >= 0 && xPos < rowCount && yPos >= 0 && yPos < colCount)
            {
                var contaminations = contaminationMap[xPos, yPos];
                contaminations.Remove(substanceId);
            }
        }

        public List<int>[,] ApplyContamination(Agent agent, List<int>[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

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

            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
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

            var substanceInContaminationTable1 = _contaminationRepository.SubstanceTable[substanceId].Item2;
            if(!substanceInContaminationTable1)
            {
                if(contaminationValues.Count == 0 ||
                   (contaminationValues.Count == 1 && contaminationValues[0] == substanceId))
                {
                    return false;
                }
                return true;
            }


            foreach (var value in contaminationValues)
            {
                var substanceInContaminationTable2 = _contaminationRepository.SubstanceTable[value].Item2;

                if (!substanceInContaminationTable2 ||
                    _contaminationRepository.ContaminationTable[substanceId][value])
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsConflicting(List<int>[,] contaminationMap, int xPos, int yPos, List<int> substanceIds)
        {
            List<int> contaminationValues = contaminationMap[xPos, yPos];
            if (contaminationValues.Count == 0)
            {
                return false;
            }

            foreach (var substanceId in substanceIds)
            {
                var substanceInContaminationTable1 = _contaminationRepository.SubstanceTable[substanceId].Item2;
                if (!substanceInContaminationTable1)
                {
                    if (contaminationValues.Count == 0 ||
                        !contaminationValues.Except(substanceIds).Any())
                    {
                        return false;
                    }

                    return true;
                }
                

                foreach (var value in contaminationValues)
                {
                    if (substanceIds.Contains(value)) continue;

                    var substanceInContaminationTable2 = _contaminationRepository.SubstanceTable[value].Item2;

                    if (!substanceInContaminationTable2 ||
                        _contaminationRepository.ContaminationTable[substanceId][value])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GetResultingSubstanceId(int substance1, int substance2)
        {
            var substance1InMergeTable = _contaminationRepository.SubstanceTable[substance1].Item2;
            var substance2InMergeTable = _contaminationRepository.SubstanceTable[substance2].Item2;
            if (substance1InMergeTable && substance2InMergeTable)
            {
                return _contaminationRepository.MergeTable[substance1][substance2];
            }

            int mergedSubstanceId = _contaminationRepository.GetMergeSubstanceValue(substance1, substance2);
            if (mergedSubstanceId != -1)
            {
                return mergedSubstanceId;
            }

            if (substance1.Equals(substance2))
            {
                return substance1;
            }

            string newSubstanceName = _contaminationRepository.SubstanceTable[substance1].Item1 + "_" + _contaminationRepository.SubstanceTable[substance2].Item1;
            _contaminationRepository.SubstanceTable.Add((newSubstanceName, false));

            var newSubstanceId = _contaminationRepository.SubstanceTable.Count - 1;
            var key = (substance1, substance2);
            if (!_contaminationRepository.MergeSubstanceTable.ContainsKey(key))
            {
                _contaminationRepository.MergeSubstanceTable.Add(key, newSubstanceId);
            }
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
            _contaminationRepository.SubstanceTable.Add((name, false));
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


        // Apply contamination for a split while taking the template into account
        public List<int>[,] ApplyContaminationSplit(Agent inputAgent, ScheduledPosition splitPositions, List<int>[,] contaminationMap)
        {
            foreach (var block in splitPositions.Template.Blocks)
            {
                foreach (var cluster in block)
                {
                    foreach (var pos in cluster.Value)
                    {
                        ApplyIfInBounds(contaminationMap, inputAgent.PositionX + pos.x, inputAgent.PositionY + pos.y, inputAgent.SubstanceId);
                    }
                }
            }
            return contaminationMap;
        }


        // Apply contamination for a merge while taking the template into account
        public List<int>[,] ApplyContaminationMerge(Agent inputAgent1, Agent inputAgent2, Agent outputAgent, ScheduledPosition mergePositions, List<int>[,] contaminationMap)
        {
            int mergeX = mergePositions.OriginX;
            int mergeY = mergePositions.OriginY;

            int size = outputAgent.GetAgentSize();

            // Reserve and populate the initial area with the new agent substance
            ApplyContaminationWithSize(outputAgent, contaminationMap);
            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
                    RemoveIfInBounds(contaminationMap, outputAgent.PositionX + i, outputAgent.PositionY + j, inputAgent1.SubstanceId);
                    RemoveIfInBounds(contaminationMap, outputAgent.PositionX + i, outputAgent.PositionY + j, inputAgent2.SubstanceId);
                }
            }

            // Apply contamination based on the template
            foreach (var block in mergePositions.Template.Blocks)
            {
                if (block.Count > 1) continue;

                foreach (var cluster in block)
                {
                    var substanceId = outputAgent.SubstanceId;

                    foreach (var pos in cluster.Value)
                    {
                        var contaminationPosX = pos.x + mergeX;
                        var contaminationPosY = pos.y + mergeY;

                        // Define the offsets for our padding of the contamination
                        var offsets = new List<(int xOffset, int yOffset)>
                        {
                            (0, 0),
                            (1, 0),
                            (-1, 0),
                            (0, 1),
                            (0, -1),
                            (1, -1),
                            (-1, 1),
                            (1, 1),
                            (-1, -1)
                        };

                        foreach (var (xOffset, yOffset) in offsets)
                        {
                            ApplyIfInBounds(contaminationMap, contaminationPosX + xOffset, contaminationPosY + yOffset, substanceId);
                        }
                    }
                }
            }

            return contaminationMap;
        }

        public List<int>[,] RemoveContaminations(List<IDropletCommand> commands, Dictionary<string, Agent> agents, List<int>[,] contaminationMap)
        {
            foreach (var command in commands)
            {
                var agent = agents[command.GetInputDroplets().First()];
                Agent reserveAgent = (Agent)agent.Clone();
                reserveAgent.PositionX = ((Move)command).PositionX;
                reserveAgent.PositionY = ((Move)command).PositionY;
                RemoveContaminationWithSize(reserveAgent, contaminationMap);
            }

            return contaminationMap;
        }

        public List<int>[,] RemoveContaminationWithSize(Agent agent, List<int>[,] contaminationMap)
        {
            var x = agent.PositionX;
            var y = agent.PositionY;

            int size = agent.GetAgentSize();

            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= size; j++)
                {
                    RemoveIfInBounds(contaminationMap, x + i, y + j, agent.SubstanceId);
                }
            }

            return contaminationMap;
        }



        public void PrintContaminationMap(List<int>[,] contaminationMap)
        {
            int rows = contaminationMap.GetLength(0);
            int cols = contaminationMap.GetLength(1);

            int maxWidth = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var content = contaminationMap[i, j].Count > 0
                        ? string.Join(", ", contaminationMap[i, j])
                        : " ";
                    maxWidth = Math.Max(maxWidth, content.Length);
                }
            }

            for (int j = 0; j < cols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    var contaminations = contaminationMap[i, j];

                    if (contaminations.Count > 0)
                    {
                        Console.Write("[");
                        Console.Write(string.Join(", ", contaminations).PadRight(maxWidth));
                        Console.Write("] ");
                    }
                    else
                    {
                        Console.Write("[ ]".PadRight(maxWidth + 3));
                    }
                }
                Console.WriteLine();
            }
        }


        public bool IsAreaContaminated(List<int>[,] contaminationMap, int substanceId, int startX, int startY, int width, int height)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            int endX = Math.Min(rowCount - 1, startX + width - 1);
            int endY = Math.Min(colCount - 1, startY + height - 1);

            for (int x = Math.Max(0, startX); x <= endX; x++)
            {
                for (int y = Math.Max(0, startY); y <= endY; y++)
                {
                    if (IsConflicting(contaminationMap, x, y, substanceId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateContaminationArea(List<int>[,] contaminationMap, int substanceId, int startX, int startY, int width, int height)
        {
            int rowCount = contaminationMap.GetLength(0);
            int colCount = contaminationMap.GetLength(1);

            int endX = Math.Min(rowCount - 1, startX + width);
            int endY = Math.Min(colCount - 1, startY + height);

            // Iterate over the rectangular area and update the contamination map
            for (int x = Math.Max(0, startX); x <= endX; x++)
            {
                for (int y = Math.Max(0, startY); y <= endY; y++)
                {
                    ApplyIfInBounds(contaminationMap, x, y, substanceId);
                }
            }
        }

        public void CopyContaminationMap(List<int>[,] source, List<int>[,] destination)
        {
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    destination[i, j] = source[i, j] != null ? new List<int>(source[i, j]) : null;
                }
            }
        }
    }
}
