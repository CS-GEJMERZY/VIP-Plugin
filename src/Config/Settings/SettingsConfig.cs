using System.Text.Json.Serialization;

namespace Config.Config
{
    public class SettingsConfig
    {
        [JsonPropertyName("Prefix")]
        public string Prefix { get; set; } = "{lightred}VIP ";
    }
}

