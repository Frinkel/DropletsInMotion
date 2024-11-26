using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;

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
        //List<(int x, int y)> AllPositions { get; }

        List<Dictionary<string, List<(int x, int y)>>> Blocks { get; set; }


        //public List<(int x, int y)> GetAllPositions()
        //{
        //    List<(int x, int y)> positions = new List<(int x, int y)>();

        //    foreach (var v in Blocks)
        //    {
        //        foreach (var kvp in v)
        //        { 
        //            positions.AddRange(kvp.Value);
        //        }
        //    }

        //    return positions;
        //}

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
