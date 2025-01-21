using System.Text.Json.Serialization;

namespace DropletsInMotion.Communication.Simulator.Models
{
    public class ActionItem
    {
        public ActionItem(string actionName, int actionOnId, int actionChange)
        {
            ActionName = actionName;
            ActionOnId = actionOnId;
            ActionChange = actionChange;
        }

        [JsonPropertyName("actionName")]
        public string ActionName { get; set; }

        [JsonPropertyName("actionOnID")]
        public int ActionOnId { get; set; }

        [JsonPropertyName("actionChange")]
        public int ActionChange { get; set; }

        public override string ToString()
        {
            return $"SimulatorAction: {{ actionName: \"{ActionName}\", actionOnID: {ActionOnId}, actionChange: {ActionChange} }}";
        }
    }
}
