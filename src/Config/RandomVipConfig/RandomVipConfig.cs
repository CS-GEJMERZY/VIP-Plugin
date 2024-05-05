using System.Text.Json.Serialization;

namespace Core.Config;

public class RandomVipConfig
{
    [JsonPropertyName("Enabled ")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("AfterRound ")]
    public int AfterRound { get; set; } = 3;

    [JsonPropertyName("MinimumPlayers ")]
    public int MinimumPlayers { get; set; } = 2;

    [JsonPropertyName("RepeatPickingMessage")]
    public int RepeatPickingMessage { get; set; } = 3;

    [JsonPropertyName("PermissionsGranted ")]
    public List<string> PermissionsGranted { get; set; } = [];

    [JsonPropertyName("PermissionExclude")]
    public List<string> PermissionsExclude { get; set; } = [];
}

