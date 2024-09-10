using DropletsInMotion.Compilers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
