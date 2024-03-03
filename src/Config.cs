using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Plugin;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Settings")]
    public Models.SettingsData Settings { get; set; } = new Models.SettingsData();

    [JsonPropertyName("Groups")]
    public List<Models.VipGroupData> Groups { get; set; } = new List<Models.VipGroupData>();

    [JsonPropertyName("RandomVIP")]
    public Models.RandomVIPData RandomVIP { get; set; } = new Models.RandomVIPData();

    public PluginConfig()
    {

        if (Groups.Count == 0)
        {
            Groups.Add(new Models.VipGroupData());
        }
    }
}


