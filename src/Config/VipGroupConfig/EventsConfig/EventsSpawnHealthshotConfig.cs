using System.Text.Json.Serialization;

namespace Config.VipGroupConfig.EventsConfig;

public class EventsSpawnHealthshotConfig
{
    [JsonPropertyName("Strip")]
    public bool Strip { get; set; } = true;

    [JsonPropertyName("Amount")]
    public int Amount { get; set; } = 1;

    [JsonPropertyName("EnableOnPistolRound")]
    public bool EnableOnPistolRound { get; set; } = false;
}
