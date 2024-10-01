using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Application.Services
{
    public interface IDependencyService
    {
        void updateExecutedNodes(List<DependencyNode> nodes, Dictionary<string, Agent> agents, double currentTime);
    }
}