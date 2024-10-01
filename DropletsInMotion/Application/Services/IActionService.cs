using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Application.Services
{
    public interface IActionService
    {
        List<BoardAction> Merge(Dictionary<string, Agent> agents, Merge mergeCommand, byte[,] contaminationMap, double time);
        List<BoardAction> SplitByVolume(Dictionary<string, Agent> agents, SplitByVolume splitCommand, byte[,] contaminationMap, double time, int direction);
        List<BoardAction> Mix(Dictionary<string, Agent> agents, Mix mixCommand, byte[,] contaminationMap, double compilerTime);
        bool InPositionToMix(Mix mixCommand, Dictionary<string, Agent> agents, List<ICommand> movesToExecute);
        bool InPositionToStore(Store storeCommand, Dictionary<string, Agent> agents, List<ICommand> movesToExecute);
    }
}
