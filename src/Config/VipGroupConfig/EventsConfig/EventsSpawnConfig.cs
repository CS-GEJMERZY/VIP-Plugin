using System.Text.Json.Serialization;
using Config.VipGroupConfig.EventsConfig;

namespace Core.Config
{
    public class EventsSpawnConfig
    {
        [JsonPropertyName("HP")]
        public int HpValue { get; set; } = 105;

        [JsonPropertyName("ArmorValue")]
        public int ArmorValue { get; set; } = 100;

        [JsonPropertyName("Helmet")]
        public bool Helmet { get; set; } = true;

        [JsonPropertyName("HelmetOnPistolRound")]
        public bool HelmetOnPistolRound { get; set; } = false;

        [JsonPropertyName("DefuseKit")]
        public bool DefuseKit { get; set; } = true;

        [JsonPropertyName("Zeus")]
        public bool Zeus { get; set; } = true;

        [JsonPropertyName("ZeusOnPistolRound")]
        public bool ZeusOnPistolRound { get; set; } = true;

        [JsonPropertyName("HealthshotAmount")]
        public int HealthshotAmount { get; set; } = 1;

        [JsonPropertyName("HealthshotOnPistolRound")]
        public bool HealthshotOnPistolRound { get; set; } = true;

        [JsonPropertyName("ExtraMoney")]
        public int ExtraMoney { get; set; } = 2000;

        [JsonPropertyName("ExtraMoneyOnPistolRound")]
        public bool ExtraMoneyOnPistolRound { get; set; } = false;

        [JsonPropertyName("Grenades")]
        public EventsSpawnGrenadesConfig Grenades { get; set; } = new();
    }
}

