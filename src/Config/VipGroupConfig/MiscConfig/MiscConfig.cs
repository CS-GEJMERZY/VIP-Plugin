using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MiscConfig
    {
        [JsonPropertyName("ExtraJumps")]
        public ExtraJumpsConfig ExtraJumps { get; set; } = new();

        [JsonPropertyName("Smoke")]
        public SmokeConfig Smoke { get; set; } = new();

        [JsonPropertyName("HealthRegen")]
        public HealthRegenConfig HealthRegen { get; set; } = new();

        [JsonPropertyName("ArmorRegen")]
        public ArmorRegenConfig ArmorRegen { get; set; } = new();

        [JsonPropertyName("FastPlant")]
        public FastPlantConfig FastPlant { get; set; } = new();

        [JsonPropertyName("FastDefuse")]
        public FastDefuseConfig FastDefuse { get; set; } = new();

        [JsonPropertyName("NoFallDamageGlobal")]
        public bool NoFallDamage { get; set; } = false;

        [JsonPropertyName("Gravity")]
        public float Gravity { get; set; } = 1.0f;

        [JsonPropertyName("Speed")]
        public float Speed { get; set; } = 1.0f;
    }
}


