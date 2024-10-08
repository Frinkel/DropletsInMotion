using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DropletsInMotion.Communication.Models
{
    public class Sensor
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("sensorId")]
        public required int SensorId { get; set; }
        [JsonPropertyName("argumentHandlers")]
        public required Dictionary<string, Handler> ArgumentHandlers { get; set; } // JSON

        public override string ToString()
        {
            return $"SimulationSensor [Name: {Name}, SensorId: {SensorId}, ArgumentHandlers: {ArgumentHandlers}]";
        }
    }
}
