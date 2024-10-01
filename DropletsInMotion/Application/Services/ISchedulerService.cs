using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Domain;
using System.Collections.Generic;

namespace DropletsInMotion.Application.Services
{
    public interface ISchedulerService
    {
        ((int optimalX, int optimalY), (int optimalX, int optimalY))? ScheduleCommand(ICommand command,
            Dictionary<string, Agent> agents, byte[,] contaminationMap);

        ((int optimalX, int optimalY), (int optimalX, int optimalY))? FindOptimalPositions(int commandX, int commandY, int d1X, int d1Y, int d2X, int d2Y, byte[,] contaminationMap, byte d1SubstanceId, byte d2SubstanceId);
    }
}