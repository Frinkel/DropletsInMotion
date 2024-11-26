using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class ContaminationException : Exception
    {
        public byte[,] ContaminationMap { get; }

        public ContaminationException(string message, byte[,] contaminationMap) : base(message)
        {
            ContaminationMap = contaminationMap;
        }

        public ContaminationException(string message, byte[,] contaminationMap, Exception innerException) : base(message, innerException)
        {
            ContaminationMap = contaminationMap;
        }
    }
}
