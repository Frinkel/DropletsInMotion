namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class DropletNotFoundException : Exception
    {
        public DropletNotFoundException(string message) : base(message)
        {
        }

        public DropletNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
