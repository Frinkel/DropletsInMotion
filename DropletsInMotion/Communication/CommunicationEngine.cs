using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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

        public async Task SendActions(List<BoardActionDto> boardActionDtoList)
        {
            await _communication.SendActions(boardActionDtoList);
        }

        public void SendRequest<T>(T request)
        {
            throw new NotImplementedException();
        }

        public async Task WaitForConnection()
        {
            Console.WriteLine("Waiting for a client to connect...");
            await _communication.WaitForConnection();
        }
    }
}
