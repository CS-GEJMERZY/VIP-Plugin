using System.Text.Json.Serialization;

namespace VIP;

public class RandomVIPData
{
    [JsonPropertyName("enabled")]
    public bool enabled { get; set; } = false;

    [JsonPropertyName("afterRound")]
    public int afterRound { get; set; } = 3;

    [JsonPropertyName("repeatPicking")]
    public int repeatPicking { get; set; } = 3;


    [JsonPropertyName("permissions")]
    public List<string> permissions { get; set; } = new List<string>();
}

