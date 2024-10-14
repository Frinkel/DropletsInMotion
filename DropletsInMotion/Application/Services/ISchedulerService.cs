using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using System.Collections.Generic;

namespace DropletsInMotion.Application.Services
{
    public interface ISchedulerService
    {
        ((int optimalX, int optimalY), (int optimalX, int optimalY))? ScheduleCommand(IDropletCommand dropletCommand,
            Dictionary<string, Agent> agents, byte[,] contaminationMap);
    }
}