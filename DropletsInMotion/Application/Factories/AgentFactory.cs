﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services;

namespace DropletsInMotion.Application.Factories
{
    public class AgentFactory : IAgentFactory
    {

        private readonly IContaminationService _contaminationService;

        public AgentFactory(IContaminationService contaminationService)
        {
            _contaminationService = contaminationService;
        }

        public Agent CreateAgent(string dropletName, int positionX, int positionY, double volume)
        {
            return new Agent(dropletName, positionX, positionY, volume, _contaminationService);
        }

        public Agent CreateAgent(string dropletName, int positionX, int positionY, double volume, byte substanceId)
        {
            return new Agent(dropletName, positionX, positionY, volume, substanceId, _contaminationService);
        }

        public Agent CreateAgent(string dropletName, int positionX, int positionY, double volume, byte substanceId, LinkedList<(int x, int y)> snakeBody)
        {
            return new Agent(dropletName, positionX, positionY, volume, substanceId, snakeBody, _contaminationService);
        }
    }
}
