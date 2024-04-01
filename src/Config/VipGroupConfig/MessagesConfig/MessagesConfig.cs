using System.Text.Json.Serialization;

namespace Core.Config
{
    public class MessagesConfig
    {
        [JsonPropertyName("Chat")]
        public ChatMessagesConfig Chat { get; set; } = new();

    }
}
