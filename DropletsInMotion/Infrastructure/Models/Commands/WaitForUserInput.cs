namespace DropletsInMotion.Infrastructure.Models.Commands
{
    public class WaitForUserInput : ICommand
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
    }
}
