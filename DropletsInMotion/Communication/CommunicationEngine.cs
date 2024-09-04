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
    internal class CommunicationEngine
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

        public void StartCommunication(bool isSimulatorCommunication)
        {
           _communication.StartCommunication();
        }

        public void SendAction(List<BoardActionDto> boardActionDtoList)
        {
            _communication.SendAction(boardActionDtoList);
        }

        public void SendRequest()
        {
            throw new NotImplementedException();
            //_communication.SendRequest();
        }
    }
}
