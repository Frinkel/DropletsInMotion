using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication;

public interface ICommunicationEngine
{
    Task SendActions(List<BoardAction> boardActionDtoList);
    Task<double> SendSensorRequest(Sensor sensor, SensorHandler handler, double time);

    event EventHandler? ClientConnected;
    event EventHandler? ClientDisconnected;
    Task<bool> SendActuatorRequest(Actuator actuator, double time);
    Task<double> SendTimeRequest();
}