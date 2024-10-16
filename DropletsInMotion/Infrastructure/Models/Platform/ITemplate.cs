using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public interface ITemplate
    {
        string Name { get; }
        int MinSize { get; }
        int MaxSize { get; }
        List<BoardAction> Actions { get; }
        List<(int id, int x, int y)> FinalPositions { get; }
        List<(int id, int x, int y)> InitialPositions { get; }
        List<BoardAction> Apply(int relativePosition, double time, double scale);
    }
}
