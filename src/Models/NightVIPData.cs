using System.Text.Json.Serialization;

namespace VIP;
public class NightVIPData
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("StartHour")]
    public int StartHour { get; set; } = 3;

    [JsonPropertyName("EndHour")]
    public int EndHour { get; set; } = 3;

    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new List<string>();
}

