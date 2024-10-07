using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Application.Services
{
    public interface IDependencyService
    {
        void updateExecutedNodes(List<IDependencyNode> nodes, Dictionary<string, Agent> agents, double currentTime);
    }
}