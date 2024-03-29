using System.Text.Json.Serialization;

namespace Config.VipGroupConfig.EventsConfig
{
    public class EventsSpawnGrenadesConfig
    {
        [JsonPropertyName("StripOnSpawn")]
        public bool StripOnSpawn { get; set; } = true;

        [JsonPropertyName("Smoke")]
        public int Smoke { get; set; } = 1;

        [JsonPropertyName("HE")]
        public int HE { get; set; } = 1;

        [JsonPropertyName("Flashbang")]
        public int Flashbang { get; set; } = 1;

        [JsonPropertyName("FireGrenade")]
        public int FireGrenade { get; set; } = 1;

        [JsonPropertyName("Decoy")]
        public int Decoy { get; set; } = 0;
    }
}
