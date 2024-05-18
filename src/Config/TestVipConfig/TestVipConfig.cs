﻿using System.Text.Json.Serialization;
using Core.Models;

namespace Core.Config;

public class TestVipConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("Mode")]
    public TestVipMode Mode { get; set; } = TestVipMode.Playtime;

    [JsonPropertyName("MaxUses")]
    public int MaxUses { get; set; } = 1;

    [JsonPropertyName("UseDelayMinutes")]
    public int UseDelayMinutes { get; set; } = 1400;

    [JsonPropertyName("Time")]
    public int Time { get; set; } = 120;

    [JsonPropertyName("UniqueGroupId")]
    public string UniqueGroupId { get; set; } = "vip1";

    [JsonPropertyName("PermissionsGranted")]
    public List<string> PermissionsGranted { get; set; } = [];

    [JsonPropertyName("PermissionsRestricted")]
    public List<string> PermissionsRestricted { get; set; } = [];
}

