using DropletsInMotion.Communication.Simulator;
using DropletsInMotion.Compilers.Models;

namespace DropletsInMotion.Communication
{
    public class CommunicationEngine : ICommunication
    {
        private ICommunication _communication;

        public CommunicationEngine(bool isSimulatorCommunication)
        {
            if (isSimulatorCommunication)
            {
                _communication = new SimulationCommunicationEngine();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task StartCommunication()
        { 
            await _communication.StartCommunication();
        }

        public async Task StopCommunication()
        {
            await _communication.StopCommunication();
        }

        public async Task SendActions(List<BoardAction> boardActionDtoList)
        {
            await _communication.SendActions(boardActionDtoList);
        }

        public async Task SendRequest(BoardSensorRequest sensorRequest)
        {
            await _communication.SendRequest(sensorRequest);
        }

        public async Task WaitForConnection()
        {
            await _communication.WaitForConnection();
        }

        public async Task<bool> IsClientConnected()
        {
            return await _communication.IsClientConnected();
        }

        public async Task<bool> IsConnectionOpen()
        {
            return await _communication.IsConnectionOpen();
        }
    }
}
