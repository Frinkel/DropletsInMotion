using DropletsInMotion.Application.Execution.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public interface ITemplate
    {
        string Name { get; }
        int MinSize { get; }
        int MaxSize { get; }

        List<BoardAction> Actions { get; }

        Dictionary<string, (int x, int y)> FinalPositions { get; }
        Dictionary<string, (int x, int y)> InitialPositions { get; }
        List<Dictionary<string, List<(int x, int y)>>> Blocks { get; set; }

        public List<BoardAction> Apply(int relativePosition, double time, double scale)
        {
            if (Actions == null || Actions.Count == 0)
            {
                throw new Exception($"Template {Name} has no action!");
            }

            List<BoardAction> finalActionDtos = new List<BoardAction>();
            foreach (BoardAction boardAction in Actions)
            {
                BoardAction newAction = new BoardAction(
                    boardAction.ElectrodeId + relativePosition,
                    boardAction.Action,
                    (boardAction.Time * scale) + time
                );
                finalActionDtos.Add(newAction);
            }
            return finalActionDtos;
        }
    }
}
