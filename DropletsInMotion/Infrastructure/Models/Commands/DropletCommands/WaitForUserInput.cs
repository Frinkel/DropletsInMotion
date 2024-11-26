namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class WaitForUserInput : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public WaitForUserInput() { }

        public override string ToString()
        {
            return $"WaitForUserInput()";
        }

        public List<string> GetInputDroplets()
        {
            return new List<string> { };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string> { };
        }
        public void Evaluate(Dictionary<string, double> variableValues)
        {
        }

        public List<string> GetInputVariables()
        {
            return new List<string> { };
        }

        public List<string> GetOutputVariables()
        {
            return new List<string> { };
        }
    }
}
