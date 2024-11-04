using System.Drawing;
using System.Text.Json.Serialization;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class MergeTemplate : ITemplate
    {
        //public SplitTemplate(string name, int minSize, int maxSize, double ratio, Dictionary<string, double> ratioRelation)
        //{
        //    Name = name;
        //    MinSize = minSize;
        //    MaxSize = maxSize;
        //    Ratio = ratio;
        //    RatioRelation = ratioRelation;
        //}


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("minSize")]
        public int MinSize { get; set; }

        [JsonPropertyName("maxSize")]
        public int MaxSize { get; set; }

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

        public MergeTemplate DeepCopy()
        {
            var copy = new MergeTemplate
            {
                Name = Name,
                MinSize = MinSize,
                MaxSize = MaxSize
            };

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


        

    }
}
