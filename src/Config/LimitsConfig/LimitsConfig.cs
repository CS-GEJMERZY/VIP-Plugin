using System.Text.Json.Serialization;

namespace Config.Limits;

public class LimitsConfig
{
    [JsonPropertyName("MaxHP")]
    public int MaxHp { get; set; } = 120;

    [JsonPropertyName("MaxMoney")]
    public int MaxMoney { get; set; } = 16000; // TO:DO
}
