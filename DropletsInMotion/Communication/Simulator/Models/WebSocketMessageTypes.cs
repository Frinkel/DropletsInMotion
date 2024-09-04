using System.Diagnostics.Contracts;

namespace DropletsInMotion.Communication.Simulator.Models
{
    public static class WebSocketMessageTypes
    {
        public const string Action = "action";
        public const string Sensor = "sensor_request";
    }

    public static class ActionTypes
    {
        public const string Electrode = "electrode";
    }
}
