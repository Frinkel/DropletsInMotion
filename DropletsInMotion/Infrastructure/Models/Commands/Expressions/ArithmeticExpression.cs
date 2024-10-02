using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropletsInMotion.Infrastructure.Models.Commands.Expressions
{
    public abstract class ArithmeticExpression
    {
        public abstract double Evaluate(Dictionary<string, double> variableValues);
    }
}
