using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Simulator.Models
{
    public class ActionQueueItem
    {
        public ActionQueueItem(ActionItem action, double time)
        {
            Action = action;
            Time = time;
        }

        [JsonPropertyName("action")]
        public ActionItem Action { get; set; }

        [JsonPropertyName("time")]
        public double Time { get; set; }

        public override string ToString()
        {
            return $"ActionQueueItem: {{ action: {Action}, time: {Time} }}";
        }
    }
}
