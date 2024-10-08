using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Communication.Models;

namespace DropletsInMotion.Communication
{
    public interface ICommunicationService
    {
        public Task StartCommunication();
        public Task StopCommunication();
        public Task SendActions(List<BoardAction> boardActionDtoList);
        public Task<double> SendRequest(Sensor sensor, Handler handler, double time);
        public Task WaitForConnection();
        //public Task<bool> IsClientConnected();
        //public Task<bool> IsConnectionOpen();
        event EventHandler? ClientDisconnected;
    }
}
