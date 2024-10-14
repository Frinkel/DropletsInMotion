using System.Text.Json.Serialization;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class Reservoir
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("outputX")]
        public required int OutputX { get; set; }

        [JsonPropertyName("outputY")]
        public required int OutputY { get; set; }

        [JsonPropertyName("dispenseSequence")]
        public required List<Dictionary<string, double>> DispenseSequence { get; set; }

        public override string ToString()
        {
            var dispenseSequenceString = string.Join(", ",
                DispenseSequence.Select(d => "{" + string.Join(", ", d.Select(kv => $"{kv.Key}: {kv.Value}")) + "}"));

            return $"Reservoir: Name={Name}, OutputX={OutputX}, OutputY={OutputY}, DispenseSequence=[{dispenseSequenceString}]";
        }
    }
}
