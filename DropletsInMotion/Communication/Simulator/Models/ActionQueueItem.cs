using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Simulator.Models
{
    public class ActionQueueItem
    {
        public ActionQueueItem(ActionItem action, decimal time)
        {
            Action = action;
            Time = time;
        }

        [JsonPropertyName("action")]
        public ActionItem Action { get; set; }

        [JsonPropertyName("time")]
        public decimal Time { get; set; }

        public override string ToString()
        {
            return $"ActionQueueItem: {{ action: {Action}, time: {Time} }}";
        }
    }
}
