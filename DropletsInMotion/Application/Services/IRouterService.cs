using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services
{
    public interface IRouterService
    {
        List<BoardAction> Route(Dictionary<string, Agent> agents, List<IDropletCommand> commands, List<int>[,] contaminationMap, double time, double? boundTime = null);

        void Initialize(Electrode[][] board, int? seed = null);
    }
}