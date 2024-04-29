using System.Text.Json.Serialization;

namespace Core.Config;

public class VipGroupConfig
{
    [JsonPropertyName("Permissions")]
    public string Permissions { get; set; } = "@vip-plugin/vip";

    [JsonPropertyName("Priority")]
    public int Priority { get; set; } = 1;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "VIP";

    [JsonPropertyName("Events")]
    public EventsConfig Events { get; set; } = new();

    [JsonPropertyName("Limits")]
    public LimitsConfig Limits { get; set; } = new();

    [JsonPropertyName("Misc")]
    public MiscConfig Misc { get; set; } = new();

    [JsonPropertyName("Messages")]
    public MessagesConfig Messages { get; set; } = new();
}

