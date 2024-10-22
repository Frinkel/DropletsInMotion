using System.Drawing;
using System.Text.Json.Serialization;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class RavelTemplate : ITemplate
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
        public double Duration { get; set; }

        public Dictionary<string, (int x, int y)> FinalPositions => throw new NotImplementedException();

        public Dictionary<string, (int x, int y)> InitialPositions => throw new NotImplementedException();

        public List<Dictionary<string, List<(int x, int y)>>> Blocks { get; set; } = new List<Dictionary<string, List<(int x, int y)>>>();

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

        //public override string ToString()
        //{
        //    // Format Actions into a readable string
        //    var actionsString = Actions != null && Actions.Any()
        //        ? string.Join(", ", Actions.Select(a => a.ToString()))
        //        : "No Actions";

        //    // Format FinalPositions into a readable string
        //    var finalPositionsString = FinalPositions != null && FinalPositions.Any()
        //        ? string.Join(", ", FinalPositions.Select(fp => $"({fp.x}, {fp.y})"))
        //        : "No Final Positions";

        //    // Format InitialPositions into a readable string
        //    var initialPositionsString = InitialPositions != null && InitialPositions.Any()
        //        ? string.Join(", ", InitialPositions.Select(ip => $"({ip.x}, {ip.y})"))
        //        : "No Initial Positions";

        //    return $"Name: {Name}, MinSize: {MinSize}, MaxSize: {MaxSize}, Actions: [{actionsString}], FinalPositions: [{finalPositionsString}], InitialPositions: [{initialPositionsString}]";
        //}

    }
}
