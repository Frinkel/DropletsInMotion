using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Models
{
    public class Sensor
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("sensorId")]
        public required int SensorId { get; set; }

        [JsonPropertyName("coordinateX")]
        public required int CoordinateX { get; set; }

        [JsonPropertyName("coordinateY")]
        public required int CoordinateY { get; set; }

        [JsonPropertyName("argumentHandlers")]
        public required Dictionary<string, SensorHandler> ArgumentHandlers { get; set; } // JSON

        public override string ToString()
        {
            return $"SimulationSensor (Name: {Name}, SensorId: {SensorId}, ArgumentHandlers: {ArgumentHandlers})";
        }
    }
}
