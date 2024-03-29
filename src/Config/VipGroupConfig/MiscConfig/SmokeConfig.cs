using System.Text.Json.Serialization;

namespace Core.Config
{
    public class SmokeConfig
    {
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("Type")]
        public Models.SmokeConfigType Type { get; set; } = Models.SmokeConfigType.Fixed;

        [JsonPropertyName("Color")]
        public string Color { get; set; } = "#FF0000";
    }
}