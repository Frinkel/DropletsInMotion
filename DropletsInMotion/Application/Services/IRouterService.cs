using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using System.Collections.Generic;

namespace DropletsInMotion.Application.Services.Routers
{
    public interface IRouterService
    {
        List<BoardAction> Route(Dictionary<string, Agent> agents, List<ICommand> commands, byte[,] contaminationMap, double time, double? boundTime = null);

        void Initialize(Electrode[][] board);

        //void UpdateAgentSubstanceId(string agent, byte substanceId);
        //byte GetAgentSubstanceId(string agent);
        //byte[,] GetContaminationMap();
        //Dictionary<string, Agent> GetAgents();
        //void UpdateContaminationMap(int x, int y, byte value);
    }
}