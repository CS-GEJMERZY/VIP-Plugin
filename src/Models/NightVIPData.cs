﻿using System.Text.Json.Serialization;

namespace Plugin.Models
{
    public class NightVIPData
    {
        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("StartHour")]
        public int StartHour { get; set; } = 22;

        [JsonPropertyName("EndHour")]
        public int EndHour { get; set; } = 8;

        [JsonPropertyName("RequiredNickPhrase")]
        public string RequiredNickPhrase { get; set; } = "Katujemy.eu";

        [JsonPropertyName("RequiredScoreboardTag")]
        public string RequiredScoreboardTag { get; set; } = "Katujemy.eu";

        [JsonPropertyName("PermissionsGranted ")]
        public List<string> PermissionsGranted { get; set; } = new List<string>();

        [JsonPropertyName("PermissionExclude")]
        public List<string> PermissionExclude { get; set; } = new List<string>();
    }
}
