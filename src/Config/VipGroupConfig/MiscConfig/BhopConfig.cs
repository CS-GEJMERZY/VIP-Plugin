using System.Text.Json.Serialization;

namespace Core.Config;

public class BhopConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("VelocityZ")]
    public float VelocityZ { get; set; } = 260.0f;
}
