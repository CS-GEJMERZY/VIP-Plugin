using System.Text.Json.Serialization;

namespace Core.Config;

public class EventsConfig
{
    [JsonPropertyName("Spawn")]
    public EventsSpawnConfig Spawn { get; set; } = new();

    [JsonPropertyName("Kill")]
    public EventsKillConfig Kill { get; set; } = new();

    [JsonPropertyName("Bomb")]
    public EventsBombConfig Bomb { get; set; } = new();

    [JsonPropertyName("Round")]
    public EventsRoundConfig Round { get; set; } = new();
}

