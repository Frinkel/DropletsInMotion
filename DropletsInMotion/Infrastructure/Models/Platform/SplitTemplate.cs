using System.Drawing;
using System.Text.Json.Serialization;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class SplitTemplate : ITemplate
    {
        public SplitTemplate(string name, int minSize, int maxSize, double ratio, Dictionary<string, double> ratioRelation)
        {
            Name = name;
            MinSize = minSize;
            MaxSize = maxSize;
            Ratio = ratio;
            RatioRelation = ratioRelation;
        }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("minSize")]
        public int MinSize { get; set; }

        [JsonPropertyName("maxSize")]
        public int MaxSize { get; set; }

        [JsonPropertyName("ratio")]
        public double Ratio { get; set; }

        [JsonPropertyName("ratioRelation")]
        public Dictionary<string, double> RatioRelation { get; set; }

        public List<BoardAction>? Actions { get; set; }

        public Dictionary<string, (int x, int y)> FinalPositions { get; set; } = new Dictionary<string, (int x, int y)>();

        public Dictionary<string, (int x, int y)> InitialPositions { get; set; } = new Dictionary<string, (int x, int y)>();

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

        public SplitTemplate DeepCopy()
        {
            var copy = new SplitTemplate(Name, MinSize, MaxSize, Ratio, new Dictionary<string, double>(RatioRelation));

            if (Actions != null)
            {
                copy.Actions = new List<BoardAction>(Actions.Select(a => new BoardAction(a.ElectrodeId, a.Action, a.Time)));
            }

            copy.FinalPositions = new Dictionary<string, (int x, int y)>(FinalPositions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            copy.InitialPositions = new Dictionary<string, (int x, int y)>(InitialPositions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            
            copy.Blocks = new List<Dictionary<string, List<(int x, int y)>>>(
                Blocks.Select(block => block.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new List<(int x, int y)>(kvp.Value.Select(pos => (pos.x, pos.y)))
                ))
            );

            return copy;
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
