using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using System.Collections.Generic;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services
{
    public interface ISchedulerService
    {
        ScheduledPosition ScheduleCommand(IDropletCommand dropletCommand,
            Dictionary<string, Agent> agents, byte[,] contaminationMap, List<ITemplate> templates);
    }
}