namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class WaitForUserInput : IDropletCommand
    {
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
    }
}
