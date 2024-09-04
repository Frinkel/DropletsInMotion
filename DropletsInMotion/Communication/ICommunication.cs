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
        public void StartCommunication();
        public void SendAction(List<BoardActionDto> boardActionDtoList);
        public void SendRequest<T>(T request);
    }
}
