using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("SqlServer")]
    public DatabaseSqlConfig SqlServer { get; set; } = new();
}
