using System.Text.Json.Serialization;

namespace Core.Config;

public class CommandConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Permissions")]
    public List<string> Permissions = [];

    [JsonPropertyName("Alias")]
    public List<string> Alias = [];
}
