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
    public EventsSpawnConfig Spawn = new();

    [JsonPropertyName("Events")]
    public EventsConfig Events = new();

    [JsonPropertyName("Limits")]
    public LimitsConfig Limits = new();

    [JsonPropertyName("Misc")]
    public MiscConfig Misc = new();

    [JsonPropertyName("Messages")]
    public MessagesConfig Messages = new();

    [JsonPropertyName("Grenades")]
    public GrenadesConfig Grenades = new();
}


