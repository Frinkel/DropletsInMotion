using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
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
        List<BoardAction> SplitByVolume(Dictionary<string, Agent> agents, SplitByVolume splitCommand, byte[,] contaminationMap, 
            double time, ((int optimalX, int optimalY), (int optimalX, int optimalY)) splitPositions);
        List<BoardAction> Mix(Dictionary<string, Agent> agents, Mix mixCommand, byte[,] contaminationMap, double compilerTime);
        bool InPositionToMix(Mix mixCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute);
        bool InPositionToStore(Store storeCommand, Dictionary<string, Agent> agents, List<IDropletCommand> movesToExecute);
        bool DropletsExistAndCommandInProgress(IDropletCommand dropletCommand, Dictionary<string, Agent> agents);

        bool InPositionToMerge(Merge mergeCommand, List<IDropletCommand> movesToExecute,
            ((int optimalX, int optimalY), (int optimalX, int optimalY)) mergePositions,
            Dictionary<string, Agent> agents);
        void MoveMergeDropletToPosition(Merge mergeCommand, List<IDropletCommand> movesToExecute, 
            Dictionary<string, Agent> agents);

        bool InPositionToSplit(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            ((int optimalX, int optimalY), (int optimalX, int optimalY)) splitPositions,
            Dictionary<string, Agent> agents);

        void MoveToSplitToFinalPositions(SplitByVolume splitCommand, List<IDropletCommand> movesToExecute,
            Dictionary<string, Agent> agents);
    }
}
