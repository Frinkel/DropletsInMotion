using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Simulator.Models
{

    public static class SensorTypes
    {
        public const string Rgb = "RGB_color";
        public const string Temperature = "temperature";
    }


    public class SimulationSensor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("ID")]
        public int Id { get; set; }

        [JsonPropertyName("sensorID")]
        public int SensorId { get; set; }

        [JsonPropertyName("positionX")]
        public int PositionX { get; set; }

        [JsonPropertyName("positionY")]
        public int PositionY { get; set; }

        [JsonPropertyName("sizeX")]
        public int SizeX { get; set; }

        [JsonPropertyName("sizeY")]
        public int SizeY { get; set; }

        [JsonPropertyName("electrodeID")]
        public int ElectrodeId { get; set; }

        [JsonPropertyName("valueRed")]
        public int ValueRed { get; set; }

        [JsonPropertyName("valueGreen")]
        public int ValueGreen { get; set; }

        [JsonPropertyName("valueBlue")]
        public int ValueBlue { get; set; }

        [JsonPropertyName("valueTemperature")]
        public float ValueTemperature { get; set; }

        public override string ToString()
        {
            return $"SimulationSensor: {{ name: \"{Name}\", type: \"{Type}\", ID: {Id}, sensorID: {SensorId}, " +
                   $"positionX: {PositionX}, positionY: {PositionY}, sizeX: {SizeX}, sizeY: {SizeY}, " +
                   $"electrodeID: {ElectrodeId}, valueRed: {ValueRed}, valueGreen: {ValueGreen}, " +
                   $"valueBlue: {ValueBlue}, valueTemperature: {ValueTemperature} }}";
        }
    }
}