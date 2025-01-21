using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Models
{
    public class Actuator
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("actuatorId")]
        public required int ActuatorId { get; set; }

        [JsonPropertyName("validArguments")]
        public required List<string> ValidArguments { get; set; }

        [JsonPropertyName("arguments")]
        public Dictionary<string, double> Arguments { get; set; } = new();

        public override string ToString()
        {
            var argumentsString = string.Join(", ", Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            return $"Actuator (Name: {Name}, ActuatorId: {ActuatorId}, Arguments: {argumentsString})";
        }
    }
}