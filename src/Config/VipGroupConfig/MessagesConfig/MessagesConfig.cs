using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MessagesConfig
    {
        [JsonPropertyName("ConnectChat")]
        public string ConnectChat { get; set; } = "VIP {playername} joined the server";

        [JsonPropertyName("DisconnectChat")]
        public string DisconnectChat { get; set; } = "VIP {playername} left the server";
    }
}
