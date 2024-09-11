using DropletsInMotion.Compilers.Models;

namespace DropletsInMotion.Communication
{
    internal interface ICommunication
    {
        public Task StartCommunication();
        public Task StopCommunication();
        public Task SendActions(List<BoardActionDto> boardActionDtoList);
        public Task SendRequest(BoardSensorRequest sensorRequest);
        public Task WaitForConnection();
        public Task<bool> IsClientConnected();
        public Task<bool> IsConnectionOpen();
    }
}
