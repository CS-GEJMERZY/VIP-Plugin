using System.Text.Json.Serialization;

namespace Core.Config
{
    public class ChatMessagesConfig
    {
        [JsonPropertyName("Connect")]
        public SingleChatMessageConfig Connect { get; set; } = new(true, "VIP {playername} joined the server", true);

        [JsonPropertyName("Disconnect")]
        public SingleChatMessageConfig Disconnect { get; set; } = new(true, "VIP {playername} left the server", true);
    }
}
