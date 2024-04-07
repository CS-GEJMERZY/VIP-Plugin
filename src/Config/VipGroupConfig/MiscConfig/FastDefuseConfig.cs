using System.Text.Json.Serialization;

namespace Core.Config
{
    public class FastDefuseConfig
    {
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("Modifier")]
        public float Modifier { get; set; } = 1.0f;
    }
}