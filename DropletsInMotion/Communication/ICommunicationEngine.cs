using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication;

public interface ICommunicationEngine
{
    event EventHandler? ClientConnected;
    event EventHandler? ClientDisconnected;
    Task SendActions(List<BoardAction> boardActionDtoList);
    Task<double> SendSensorRequest(Sensor sensor, SensorHandler handler, double time);
    Task<bool> SendActuatorRequest(Actuator actuator, double time);
    Task<double> SendTimeRequest();
    Task StopCommunication();
}