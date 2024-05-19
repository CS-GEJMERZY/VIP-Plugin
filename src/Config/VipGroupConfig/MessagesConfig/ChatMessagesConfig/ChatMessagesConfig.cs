using System.Text.Json.Serialization;

namespace Core.Config;

public class ChatMessagesConfig
{
    [JsonPropertyName("Connect")]
    public SingleChatMessageConfig Connect { get; set; } = new()
    {
        Enabled = true,
        Message = "VIP {playername} joined the server",
    };

    [JsonPropertyName("Disconnect")]
    public SingleChatMessageConfig Disconnect { get; set; } = new()
    {
        Enabled = true,
        Message = "VIP {playername} left the server",
    };
}
