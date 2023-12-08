using System.Text.Json.Serialization;

public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "[VIP-Plugin] ";
}