using DropletsInMotion.Infrastructure.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Application.Execution
{
    public interface IExecutionEngine
    {
        Task Execute(List<ICommand> commands, Dictionary<string, Droplet> droplets, string platformPath);
        Dictionary<string, Agent> Agents { get; }
        double Time { get; set; }
    }
}
