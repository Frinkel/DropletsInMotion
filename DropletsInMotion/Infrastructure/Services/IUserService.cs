using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Infrastructure.Services
{
    public interface IUserService
    {
        public enum CommunicationType
        {
            Simulator,
            Physical,
            NotSet,
        }

        string PlatformPath { get; set; }
        string ProgramPath { get; set; }
        public CommunicationType Communication { get; set; }
        event EventHandler? CommunicationTypeChanged;
    }
}
