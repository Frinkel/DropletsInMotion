using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class RuntimeException : Exception
    {
        public RuntimeException(string message) : base(message)
        {
        }

        public RuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
