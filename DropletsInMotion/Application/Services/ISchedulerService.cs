﻿using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Platform;

namespace DropletsInMotion.Application.Services
{
    public interface ISchedulerService
    {
        ScheduledPosition ScheduleCommand(IDropletCommand dropletCommand,
            Dictionary<string, Agent> agents, List<int>[,] contaminationMap, List<ITemplate> templates);
    }
}