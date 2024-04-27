using System.Text.Json.Serialization;

namespace Core.Config;

public class EventsBombConfig
{
    [JsonPropertyName("PlantMoney")]
    public int BombPlantMoney { get; set; } = 500;

    [JsonPropertyName("DefuseMoney")]
    public int BombDefuseMoney { get; set; } = 500;
}

