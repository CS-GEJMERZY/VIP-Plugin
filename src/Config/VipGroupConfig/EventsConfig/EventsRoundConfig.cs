using System.Text.Json.Serialization;
namespace Core.Config
{
    public class EventsRoundConfig
    {
        [JsonPropertyName("WinMoney")]
        public int WinMoney { get; set; } = 1000;

        [JsonPropertyName("LoseMoney")]
        public int LoseMoney { get; set; } = 0; // TO:DO
    }
}

