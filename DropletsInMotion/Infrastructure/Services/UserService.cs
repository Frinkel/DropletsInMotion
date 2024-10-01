using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Infrastructure.Services
{
    public class UserService : IUserService
    {

        public string? PlatformPath { get; set; }
        public string? ProgramPath { get; set; }
        public IUserService.CommunicationType Communication { get; set; } = IUserService.CommunicationType.Simulator;

    }
}
