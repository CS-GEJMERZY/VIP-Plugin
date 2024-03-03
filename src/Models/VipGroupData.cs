using System.Reflection;
using System.Text.Json.Serialization;

[AttributeUsage(AttributeTargets.Property)]
public class NonNegativeAttribute : Attribute
{
    public int DefaultValue { get; }

    public NonNegativeAttribute(int defaultValue = 0)
    {
        DefaultValue = defaultValue;
    }

    public void Validate(ref int value)
    {
        value = value < 0 ? DefaultValue : value;
    }
}

public class VipGroupData
{
    [JsonPropertyName("Permissions")]
    public string Permissions { get; set; } = "@vip-plugin/vip";

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "VIP";

    [NonNegative]
    [JsonPropertyName("ExtraJumps")]
    public int ExtraJumps { get; set; } = 1;

    [NonNegative]
    [JsonPropertyName("NoFallDamage")]
    public bool NoFallDamage { get; set; } = true;

    [NonNegative]
    [JsonPropertyName("SpawnHP")]
    public int SpawnHP { get; set; } = 105;

    [NonNegative]
    [JsonPropertyName("SpawnArmor")]
    public int SpawnArmor { get; set; } = 100;

    [JsonPropertyName("SpawnHelmet")]
    public bool SpawnHelmet { get; set; } = true;

    [JsonPropertyName("SpawnDefuser")]
    public bool SpawnDefuser { get; set; } = true;

    [NonNegative]
    [JsonPropertyName("SpawnMoney")]
    public int SpawnMoney { get; set; } = 2000;

    [JsonPropertyName("GiveSpawnMoneyPistolRound")]
    public bool GiveSpawnMoneyPistolRound { get; set; } = false;

    [NonNegative]
    [JsonPropertyName("RoundWonMoney")]
    public int RoundWonMoney { get; set; } = 1000;

    [NonNegative]
    [JsonPropertyName("KillMoney")]
    public int KillMoney { get; set; } = 200;

    [NonNegative]
    [JsonPropertyName("HeadshotKillMoney")]
    public int HeadshotKillMoney { get; set; } = 300;

    [NonNegative]
    [JsonPropertyName("BombPlantMoney")]
    public int BombPlantMoney { get; set; } = 500;

    [NonNegative]
    [JsonPropertyName("BombDefuseMoney")]
    public int BombDefuseMoney { get; set; } = 500;


    [NonNegative]
    [JsonPropertyName("KillHP")]
    public int KillHP { get; set; } = 2;

    [NonNegative]
    [JsonPropertyName("HeadshotKillHP")]
    public int HeadshotKillHP { get; set; } = 5;

    [NonNegative]
    [JsonPropertyName("MaxHP")]
    public int MaxHP { get; set; } = 120;

    [JsonPropertyName("ConnectMessage")]
    public string ConnectMessage { get; set; } = "VIP {playername} joined the server";

    [JsonPropertyName("DisconnectMessage")]
    public string DisconnectMessage { get; set; } = "VIP {playername} left the server";

    [NonNegative]
    [JsonPropertyName("SmokeGrenade")]
    public int SmokeGrenade { get; set; } = 1;

    [NonNegative]
    [JsonPropertyName("HEGrenade")]
    public int HEGrenade { get; set; } = 1;

    [NonNegative]
    [JsonPropertyName("FlashGrenade")]
    public int FlashGrenade { get; set; } = 1;

    [NonNegative]
    [JsonPropertyName("Molotov")]
    public int Molotov { get; set; } = 1;

    [NonNegative]
    [JsonPropertyName("DecoyGrenade")]
    public int DecoyGrenade { get; set; } = 0;

    [JsonPropertyName("Zeus")]
    public bool Zeus { get; set; } = true;

    internal void ValidateData()
    {
        foreach (var property in GetType().GetProperties())
        {
            if (property.IsDefined(typeof(NonNegativeAttribute), inherit: false))
            {
                if (property.PropertyType == typeof(int))
                {
                    var value = (int)property.GetValue(this)!;
                    var attribute = (NonNegativeAttribute)property.GetCustomAttribute(typeof(NonNegativeAttribute))!;
                    attribute.Validate(ref value);
                    property.SetValue(this, value);
                }
            }
        }
    }

    public VipGroupData()
    {
        ValidateData();
    }
}