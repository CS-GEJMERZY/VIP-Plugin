using System.Text.Json.Serialization;

namespace Config.Events;

public class EventsConfig
{
    [JsonPropertyName("KillConfig")]
    public EventsKillConfig KillConfig { get; set; } = new();

    [JsonPropertyName("BombConfig")]
    public EventsBombConfig BombConfig { get; set; } = new();

    [JsonPropertyName("RoundConfig")]
    public EventsRoundConfig RoundConfig { get; set; } = new();
}