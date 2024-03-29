﻿using System.Text.Json.Serialization;

namespace Core.Config
{
    public class ExtraJumpsConfig
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("VelocityZ")]
        public double VelocityZ { get; set; } = 260.0;
    }
}