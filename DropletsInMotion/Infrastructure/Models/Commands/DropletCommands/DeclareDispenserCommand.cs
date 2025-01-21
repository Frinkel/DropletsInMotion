namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class DeclareDispenserCommand : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DispenserIdentifier { get; set; }
        public string DispenserName { get; set; }
        public string Substance { get; set; }

        public DeclareDispenserCommand(string dispenserIdentifier, string dispenserName, string substance)
        {
            DispenserIdentifier = dispenserIdentifier;
            DispenserName = dispenserName;
            Substance = substance;
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
        }

        public override string ToString()
        {
            return $"DeclareDispenser( ${DispenserIdentifier}, ${DispenserName}, ${Substance})";
        }

        public List<string> GetInputVariables()
        {
            return new List<string>() { DispenserIdentifier };
        }

        public List<string> GetOutputVariables()
        {
            return new List<string>() { DispenserIdentifier };
        }

        public List<string> GetInputDroplets()
        {
            return new List<string>() { };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string>() { };
        }
    }
}
