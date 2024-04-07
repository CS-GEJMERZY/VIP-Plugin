using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MiscConfig
    {
        [JsonPropertyName("ExtraJumps")]
        public ExtraJumpsConfig ExtraJumps { get; set; } = new();

        [JsonPropertyName("Smoke")]
        public SmokeConfig Smoke { get; set; } = new();

        [JsonPropertyName("HpRegen")]
        public HpRegenConfig HpRegen { get; set; } = new();

        [JsonPropertyName("NoFallDamageGlobal")]
        public bool NoFallDamage { get; set; } = false;

        [JsonPropertyName("Gravity")]
        public float Gravity { get; set; } = 1.0f;

        [JsonPropertyName("Speed")]
        public float Speed { get; set; } = 1.0f;
    }
}


