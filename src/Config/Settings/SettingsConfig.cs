using System.Text.Json.Serialization;

namespace Core.Config;

public class SettingsConfig
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{lightred}VIP ";

    [JsonPropertyName("EnableDatabaseVips")]
    public bool EnableDatabaseVips { get; set; } = false;

    [JsonPropertyName("Database")]
    public DatabaseConfig Database { get; set; } = new();
}
