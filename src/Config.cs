using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace VIP;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Settings")]
    public SettingsData Settings { get; set; } = new SettingsData();

    [JsonPropertyName("Groups")]
    public List<VIPGroup> Groups { get; set; } = new List<VIPGroup>();

    public PluginConfig()
    {

        if (Groups.Count == 0)
        {
            Groups.Add(new VIPGroup());
        }
    }
}


