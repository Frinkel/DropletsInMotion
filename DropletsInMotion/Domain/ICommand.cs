using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Domain
{
    public interface ICommand
    {
        List<string> GetInputDroplets();
        List<string> GetOutputDroplets();
    }
}
