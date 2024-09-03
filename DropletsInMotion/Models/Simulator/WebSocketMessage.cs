﻿
namespace DropletsInMotion.Models.Simulator
{
    public class WebSocketMessage<T>
    {
        public WebSocketMessage(string type, T data)
        {
            Type = type;
            Data = data;
        }

        public string? Type { get; set; }
        public T? Data { get; set; }
    }
}