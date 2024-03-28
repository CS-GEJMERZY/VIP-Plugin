﻿using System.Text.Json.Serialization;

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

        // TO:DO - zeus on pistol

        [JsonPropertyName("ExtraMoney")]
        public int ExtraMoney { get; set; } = 2000;

        [JsonPropertyName("ExtraMoneyOnPistolRound")]
        public bool ExtraMoneyOnPistolRound { get; set; } = false;
    }
}
