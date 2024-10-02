using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Communication;

public interface ICommunicationEngine
{
    Task SendActions(List<BoardAction> boardActionDtoList);
    Task SendRequest(BoardSensorRequest sensorRequest);
    event EventHandler? ClientConnected;

    event EventHandler? ClientDisconnected;
}