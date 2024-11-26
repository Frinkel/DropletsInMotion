using System;
using System.Collections.Generic;
using System.Linq;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Models.Commands.Expressions;

namespace DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands
{
    public class ActuatorCommand : IDropletCommand
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string DropletName { get; }
        public string ActuatorName { get; }
        public Dictionary<string, double> KeyValuePairs { get; }

        // Constructor for ActuatorCommand
        public ActuatorCommand(string dropletName, string actuatorName, Dictionary<string, double> keyValuePairs)
        {
            DropletName = dropletName;
            ActuatorName = actuatorName;
            KeyValuePairs = keyValuePairs;
        }

        public void Evaluate(Dictionary<string, double> variableValues)
        {
            // Implement logic for evaluating the command if necessary, using variableValues
        }

        public List<string> GetInputVariables()
        {
            var res = new List<string>() { ActuatorName };

            if (!string.IsNullOrEmpty(DropletName))
            {
                res.Add(DropletName);
            }

            return res;
        }

        public List<string> GetOutputVariables()
        {
            var res = new List<string>() { ActuatorName };

            if (!string.IsNullOrEmpty(DropletName))
            {
                res.Add(DropletName);
            }

            return res;
        }

        public List<string> GetInputDroplets()
        {

            var res = new List<string>();

            if (!string.IsNullOrEmpty(DropletName))
            {
                res.Add(DropletName);
            }

            return res;
        }

        public List<string> GetOutputDroplets()
        {
            var res = new List<string>();

            if (!string.IsNullOrEmpty(DropletName))
            {
                res.Add(DropletName);
            }

            return res;
        }

        public override string ToString()
        {
            var kvpString = string.Join(", ", KeyValuePairs.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return !string.IsNullOrEmpty(DropletName)
                ? $"Actuator({DropletName}, {ActuatorName}, {kvpString})"
                : $"Actuator({ActuatorName}, {kvpString})";
        }
    }
}
