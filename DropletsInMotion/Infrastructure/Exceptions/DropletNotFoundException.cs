using DropletsInMotion.Application.Models;
using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class DropletNotFoundException : Exception
    {
        //private string DropletName { get; }

        public DropletNotFoundException(string message) : base(message)
        {
        }

        public DropletNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
