using System.Text.Json.Serialization;

namespace Core.Config
{
    public class HpRegenConfig
    {
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("Interval")]
        public int Interval { get; set; } = 15;

        [JsonPropertyName("Delay")]
        public int Delay { get; set; } = 10;

        [JsonPropertyName("Amount")]
        public int Amount { get; set; } = 5;
    }
}

