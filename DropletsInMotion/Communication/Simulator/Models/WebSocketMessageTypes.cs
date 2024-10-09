namespace DropletsInMotion.Communication.Simulator.Models
{
    public static class WebSocketMessageTypes
    {
        public const string Action = "action";
        public const string Sensor = "sensor_request";
        public const string Actuator = "actuator_request";
    }
    public static class WebSocketResponseTypes
    {
        public const string Sensor = "sensor_response";
    }

    public static class ActionTypes
    {
        public const string Electrode = "electrode";
    }
}
