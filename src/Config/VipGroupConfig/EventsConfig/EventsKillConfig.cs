using System.Text.Json.Serialization;
namespace Core.Config
{
    public class EventsKillConfig
    {
        [JsonPropertyName("HP")]
        public int Hp { get; set; } = 2;

        [JsonPropertyName("HeadshotHP")]
        public int HeadshotHp { get; set; } = 3;

        [JsonPropertyName("Money")]
        public int Money { get; set; } = 200;

        [JsonPropertyName("HeadshotMoney")]
        public int HeadshotMoney { get; set; } = 300;
    }
}

