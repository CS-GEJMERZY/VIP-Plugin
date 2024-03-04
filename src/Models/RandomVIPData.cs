using System.Text.Json.Serialization;

namespace Plugin.Models
{
    public class RandomVIPData
    {
        [JsonPropertyName("Enabled ")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("AfterRound ")]
        public int AfterRound { get; set; } = 3;

        [JsonPropertyName("MinimumPlayers ")]
        public int MinimumPlayers { get; set; } = 2;

        [JsonPropertyName("RepeatPicking ")]
        public int RepeatPicking { get; set; } = 3;

        [JsonPropertyName("PermissionsGranted ")]
        public List<string> PermissionsGranted { get; set; } = new List<string>();

        [JsonPropertyName("PermissionExclude")]
        public List<string> PermissionExclude { get; set; } = new List<string>();
    }
}

