using System.Drawing;
using System.Text.Json.Serialization;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class ShrinkTemplate : ITemplate
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("minSize")]
        public required int MinSize { get; set; }

        [JsonPropertyName("maxSize")]
        public required int MaxSize { get; set; }
        [JsonPropertyName("direction")]
        public required string Direction { get; set; }

        public List<BoardAction>? Actions { get; set; }

        public Dictionary<string, (int x, int y)> FinalPositions => throw new NotImplementedException();

        public Dictionary<string, (int x, int y)> InitialPositions => throw new NotImplementedException();

        public List<(int x, int y)> ContaminationPositions => throw new NotImplementedException();

        public List<Dictionary<string, List<(int x, int y)>>> Blocks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<BoardAction> Apply(int relativePosition, double time, double scale)
        {
            if (Actions == null || Actions.Count == 0)
            {
                throw new Exception($"Template {Name} has no action!");
            }


            List<BoardAction> finalActionDtos = new List<BoardAction>();
            foreach (BoardAction boardAction in Actions)
            {
                BoardAction newAction = new BoardAction(boardAction.ElectrodeId + relativePosition, boardAction.Action,
                    (boardAction.Time * scale) + time);
                finalActionDtos.Add(newAction);
            }
            return finalActionDtos;
        }


    }
}
