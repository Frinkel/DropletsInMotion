namespace DropletsInMotion.Communication.Simulator.Models
{
    public class RequestWrapper
    {
        public RequestWrapper(object data, double time)
        {
            Data = data;
            Time = time;
        }
        public double Time { get; set; }
        public object Data { get; set; }
    }
}
