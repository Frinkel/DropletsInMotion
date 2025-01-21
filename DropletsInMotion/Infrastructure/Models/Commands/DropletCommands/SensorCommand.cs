namespace DropletsInMotion.Infrastructure.Models.Commands.DropletCommands
{
    public class SensorCommand : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string SensorName { get; }
        public string Argument { get; }
        public string VariableName { get; }
        public string DropletName { get; }

        public SensorCommand(string variableName, string sensorName, string argument, string dropletName)
        {
            SensorName = sensorName;
            Argument = argument;
            VariableName = variableName;
            DropletName = dropletName;
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
        }

        public List<string> GetInputVariables()
        {
            var res = new List<string>() { SensorName };
            res.AddRange(GetInputDroplets());

            return res;
        }

        public List<string> GetOutputVariables()
        {
            var res = new List<string>() { SensorName };
            res.AddRange(GetOutputDroplets());

            return res;
        }

        public List<string> GetInputDroplets()
        {
            return new List<string>() { DropletName };
        }

        public List<string> GetOutputDroplets()
        {
            return new List<string>() { DropletName };
        }

        public override string ToString()
        {
            return $"{VariableName} = Sensor({SensorName}, {Argument})";
        }
    }
}