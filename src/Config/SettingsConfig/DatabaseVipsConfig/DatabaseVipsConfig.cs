using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseVipsConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("Database")]
    public DatabaseSqlConfig SqlConfig { get; set; } = new();

    [JsonPropertyName("Commands")]
    public DatabaseCommandsConfig Commands { get; set; } = new();
}
