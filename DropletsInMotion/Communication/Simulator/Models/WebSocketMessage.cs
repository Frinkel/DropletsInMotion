namespace DropletsInMotion.Communication.Simulator.Models
{
    public class WebSocketMessage<T>
    {
        public WebSocketMessage(string type, T data)
        {
            Type = type;
            Data = data;
            RequestId = Guid.NewGuid();
        }

        public Guid RequestId { get; set; }
        public string? Type { get; set; }
        public T? Data { get; set; }
    }
}