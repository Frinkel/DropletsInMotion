using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication
{
    public interface ICommunicationService
    {
        event EventHandler? ClientDisconnected;
        public Task StartCommunication();
        public Task StopCommunication();
        public Task SendActions(List<BoardAction> boardActionDtoList);
        public Task<double> SendSensorRequest(Sensor sensor, SensorHandler sensorHandler, double time);
        public Task WaitForConnection();
        Task<bool> SendActuatorRequest(Actuator actuator, double time);
        Task<double> SendTimeRequest();
    }
}
