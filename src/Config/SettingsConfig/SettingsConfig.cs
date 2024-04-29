using System.Text.Json.Serialization;

namespace Core.Config;

public class SettingsConfig
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{lightred}VIP ";

    [JsonPropertyName("DatabaseVipsConfig")]
    public DatabaseVipsConfig DatabaseVips { get; set; } = new();
}
