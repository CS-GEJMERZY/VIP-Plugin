using System.Text.Json.Serialization;

namespace Core.Config;

public class SingleChatMessageConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("Message")]
    public string Message { get; set; } = "MSG";

    [JsonPropertyName("DontBroadcast")]
    public bool DontBroadcast { get; set; } = true;
}
