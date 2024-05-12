using System.Text.Json.Serialization;
using Core.Models;

namespace Core.Config;

public class TestVipConfig
{
    [JsonPropertyName("Enabled ")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("Mode")]
    public TestVipMode Mode { get; set; } = TestVipMode.Playtime;

    [JsonPropertyName("Time ")]
    public int Time { get; set; } = 120;

    [JsonPropertyName("UniqueGroupId ")]
    public string UniqueGroupId { get; set; } = "vip1";

    [JsonPropertyName("FlagsGranted ")]
    public List<string> FlagsGranted { get; set; } = [];

    [JsonPropertyName("FlagsRestrict ")]
    public List<string> FlagsRestrict { get; set; } = [];
}

