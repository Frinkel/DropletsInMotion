﻿using System.Text.Json.Serialization;
using DropletsInMotion.Application.Execution.Models;

namespace DropletsInMotion.Infrastructure.Models.Platform
{
    public class DeclareTemplate : ITemplate
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("minSize")]
        public required int MinSize { get; set; }

        [JsonPropertyName("maxSize")]
        public required int MaxSize { get; set; }

        public List<BoardAction>? Actions { get; set; }

        public Dictionary<string, (int x, int y)> FinalPositions => throw new NotImplementedException();

        public Dictionary<string, (int x, int y)> InitialPositions { get; set; } = new Dictionary<string, (int x, int y)>();

        public List<(int x, int y)> ContaminationPositions => throw new NotImplementedException();

        public List<Dictionary<string, List<(int x, int y)>>> Blocks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    }
}
