using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication;

public interface ICommunicationEngine
{
    Task SendActions(List<BoardAction> boardActionDtoList);
    Task<double> SendSensorRequest(string sensorName, string argument, double time);

    event EventHandler? ClientConnected;
    event EventHandler? ClientDisconnected;
    Task<bool> SendActuatorRequest(Actuator actuator, double time);
}