using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Communication;

public interface ICommunicationEngine
{
    Task SendActions(List<BoardAction> boardActionDtoList);
    Task<double> SendRequest(string sensorName, string argument, double time);

    event EventHandler? ClientConnected;
    event EventHandler? ClientDisconnected;
}