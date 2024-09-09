using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Simulator.Models
{
    public class SensorRequest
    {
        public SensorRequest(int id, decimal time)
        {
            Id = id;
            Time = time;
        }

        public int Id { get; set; }
        public decimal Time { get; set; }
        public override string ToString()
        {
            return $"SensorRequest: {{ Id: {Id}, Time: {Time} }}";
        }
    }
}
