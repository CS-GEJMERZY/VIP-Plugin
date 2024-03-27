using System.Text.Json.Serialization;

namespace Core.Models;

public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "[VIP-Plugin] ";
}
