using DropletsInMotion.Application.Models;

namespace DropletsInMotion.Application.Execution
{
    public interface IExecutionEngine
    {
        Task Execute();
        Dictionary<string, Agent> Agents { get; }
        double Time { get; set; }
    }
}
