using DropletsInMotion.Application.ExecutionEngine.Models;

namespace DropletsInMotion.Communication
{
    public interface ICommunicationService
    {
        public Task StartCommunication();
        public Task StopCommunication();
        public Task SendActions(List<BoardAction> boardActionDtoList);
        public Task SendRequest(BoardSensorRequest sensorRequest);
        public Task WaitForConnection();
        public Task<bool> IsClientConnected();
        public Task<bool> IsConnectionOpen();
    }
}
