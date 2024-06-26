﻿using System.Text.Json.Serialization;

namespace Core.Config;

public class ExtraJumpsConfig
{
    [JsonPropertyName("Amount")]
    public int Amount { get; set; } = 0;

    [JsonPropertyName("VelocityZ")]
    public float VelocityZ { get; set; } = 260.0f;

    [JsonPropertyName("NoFallDamage")]
    public bool NoFallDamage { get; set; } = true;
}
