using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models;

namespace DropletsInMotion.Application.Services
{
    public interface IDependencyService
    {
        void updateExecutedNodes(List<IDependencyNode> nodes, Dictionary<string, Agent> agents, double currentTime);
    }
}