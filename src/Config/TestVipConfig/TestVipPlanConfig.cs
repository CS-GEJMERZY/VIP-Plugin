using System.Text.Json.Serialization;

namespace Core.Config;

public class TestVipPlanConfig
{
    [JsonPropertyName("MaxUses")]
    public int MaxUses { get; set; } = 3;

    [JsonPropertyName("Delay")]
    public int Delay { get; set; } = 1440;
}

