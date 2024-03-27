using System.Text.Json.Serialization;

namespace Config.Events;

public class EventsConfig
{
    [JsonPropertyName("KillConfig")]
    public EventsKillConfig KillConfig = new();

    [JsonPropertyName("BombConfig")]
    public EventsBombConfig BombConfig = new();

    [JsonPropertyName("RoundConfig")]
    public EventsRoundConfig RoundConfig = new();
}