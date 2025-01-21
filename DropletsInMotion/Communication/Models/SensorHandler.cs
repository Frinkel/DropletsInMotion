using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Models
{
    public class SensorHandler
    {
        [JsonPropertyName("request")]
        public required object Request { get; set; }
        [JsonPropertyName("response")]
        public required string Response { get; set; }
    }
}
