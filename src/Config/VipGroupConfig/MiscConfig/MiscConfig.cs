using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MiscConfig
    {
        [JsonPropertyName("ExtraJumps")]
        public ExtraJumpsConfig ExtraJumps { get; set; } = new();

        [JsonPropertyName("NoFallDamage")]
        public bool NoFallDamage { get; set; } = true;
    }
}


