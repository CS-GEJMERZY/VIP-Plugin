using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Core.Config;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Settings")]
    public Models.SettingsData Settings { get; set; } = new Models.SettingsData();

    [JsonPropertyName("VIPGroups")]
    public List<VIPGroupConfig> VIPGroups { get; set; } = [];

    [JsonPropertyName("RandomVIP")]
    public Models.RandomVIPData RandomVIP { get; set; } = new Models.RandomVIPData();

    [JsonPropertyName("NightVIP")]
    public Models.NightVIPData NightVIP { get; set; } = new Models.NightVIPData();

    public PluginConfig()
    {
        if (VIPGroups.Count == 0)
        {
            VIPGroups.Add(new VIPGroupConfig());
        }
    }
}


