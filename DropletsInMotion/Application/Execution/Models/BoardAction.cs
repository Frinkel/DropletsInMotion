namespace DropletsInMotion.Application.Execution.Models
{
    public class BoardAction
    {
        public int ElectrodeId { get; set; }
        public int Action { get; set; }
        public double Time { get; set; }

        public BoardAction(int electrodeId, int action, double time)
        {
            ElectrodeId = electrodeId;
            Action = action;
            Time = time;
        }

        public override string ToString()
        {
            return $"BoardAction(ElectrodeId: {ElectrodeId}, Action: {Action}, Time: {Time})";
        }
    }
}
