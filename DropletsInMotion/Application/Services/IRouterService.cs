using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using System.Collections.Generic;
using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services.Routers
{
    public interface IRouterService
    {
        List<BoardAction> Route(Dictionary<string, Agent> agents, List<IDropletCommand> commands, byte[,] contaminationMap, double time, double? boundTime = null);

        void Initialize(Electrode[][] board, int? seed = null);

        //void UpdateAgentSubstanceId(string agent, byte substanceId);
        //byte GetAgentSubstanceId(string agent);
        //byte[,] GetContaminationMap();
        //Dictionary<string, Agent> GetAgents();
        //void UpdateContaminationMap(int x, int y, byte value);
    }
}