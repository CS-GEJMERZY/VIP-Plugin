using System.Text.Json.Serialization;

namespace Core.Config;

public class SettingsConfig
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{lightred}►►  {default}";

    [JsonPropertyName("Database")]
    public DatabaseConfig Database { get; set; } = new();

    [JsonPropertyName("DatabaseVipsConfig")]
    public DatabaseVipsConfig DatabaseVips { get; set; } = new();
}
