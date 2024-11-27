using DropletsInMotion.Infrastructure.Models.Commands;

namespace DropletsInMotion.Infrastructure.Exceptions
{
    public class ContaminationException : Exception
    {
        public List<int>[,] ContaminationMap { get; }

        public ContaminationException(string message, List<int>[,] contaminationMap) : base(message)
        {
            ContaminationMap = contaminationMap;
        }

        public ContaminationException(string message, List<int>[,] contaminationMap, Exception innerException) : base(message, innerException)
        {
            ContaminationMap = contaminationMap;
        }
    }
}
