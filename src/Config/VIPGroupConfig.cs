using System.Text.Json.Serialization;
using Config.Events;
using Config.EventsConfig;
using Config.Limits;
using Config.MiscConfig;

namespace Core.Config;

public class VIPGroupConfig
{
    [JsonPropertyName("Permissions")]
    public string Permissions { get; set; } = "@vip-plugin/vip";

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "VIP";

    [JsonPropertyName("Spawn")]
    public EventsSpawnConfig Spawn { get; set; } = new();

    [JsonPropertyName("Events")]
    public EventsConfig Events { get; set; } = new();

    [JsonPropertyName("Limits")]
    public LimitsConfig Limits { get; set; } = new();

    [JsonPropertyName("Misc")]
    public MiscConfig Misc { get; set; } = new();

    [JsonPropertyName("Messages")]
    public MessagesConfig Messages { get; set; } = new();

    [JsonPropertyName("Grenades")]
    public GrenadesConfig Grenades { get; set; } = new();
}


