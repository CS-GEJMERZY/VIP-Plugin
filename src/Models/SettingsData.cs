using System.Text.Json.Serialization;

namespace Plugin.Models
{
    public class SettingsData
    {
        [JsonPropertyName("Prefix")]
        public string Prefix { get; set; } = "[VIP-Plugin] ";
    }
}
