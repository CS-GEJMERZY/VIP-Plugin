using System.Text.Json.Serialization;

namespace Config.MiscConfig;

public class MiscConfig
{
    [JsonPropertyName("ExtraJumps")]
    public int ExtraJumps { get; set; } = 1;

    [JsonPropertyName("NoFallDamage")]
    public bool NoFallDamage { get; set; } = true;
}
