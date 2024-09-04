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
        public Task SendActions(List<BoardActionDto> boardActionDtoList);
        public void SendRequest<T>(T request);
        public Task WaitForConnection();
    }
}
