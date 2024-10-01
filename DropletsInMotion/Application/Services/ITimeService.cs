using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Application.Services
{
    public interface ITimeService
    {
        double? CalculateBoundTime(double currentTime, double? boundTime);
    }

}
