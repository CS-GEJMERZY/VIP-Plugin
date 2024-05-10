using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseVipsConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("ReloadPlayersOnSpawn")]
    public bool ReloadPlayersOnSpawn { get; set; } = true;

    [JsonPropertyName("Commands")]
    public DatabaseCommandsConfig Commands { get; set; } = new();
}
