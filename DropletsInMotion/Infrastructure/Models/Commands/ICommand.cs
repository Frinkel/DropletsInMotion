namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public interface ICommand
    {
        void Evaluate(Dictionary<string, double> variableValues);
        public List<string> GetInputVariables();
        public List<string> GetOutputVariables();

        public int Line { get; set; }
        public int Column { get; set; }

    }
}