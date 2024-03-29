using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MiscConfig
    {
        [JsonPropertyName("ExtraJumps")]
        public ExtraJumpsConfig ExtraJumps { get; set; } = new();

        [JsonPropertyName("NoFallDamageGlobal")]
        public bool NoFallDamage { get; set; } = true;

        [JsonPropertyName("Smoke")]
        public SmokeConfig Smoke { get; set; } = new();
    }
}


