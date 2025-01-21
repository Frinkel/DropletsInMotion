using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Models
{
    public class ActuatorDto
    {
        public ActuatorDto(int actuatorId, Dictionary<string, double> arguments)
        {
            ActuatorId = actuatorId;
            Arguments = arguments;
        }

        [JsonPropertyName("actuatorId")]
        public int ActuatorId { get; set; }

        [JsonPropertyName("arguments")]
        public Dictionary<string, double> Arguments { get; set; }
    }
}
