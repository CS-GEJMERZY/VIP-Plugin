using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Core.Config
{
    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("Settings")]
        public Models.SettingsData Settings { get; set; } = new Models.SettingsData();

        [JsonPropertyName("VIPGroups")]
        public List<VipGroupConfig> VIPGroups { get; set; } = [];

        [JsonPropertyName("RandomVIP")]
        public RandomVipConfig RandomVip { get; set; } = new();

        [JsonPropertyName("NightVIP")]
        public NightVipConfig NightVip { get; set; } = new();

        public PluginConfig()
        {
            if (VIPGroups.Count == 0)
            {
                VIPGroups.Add(new VipGroupConfig());
            }
        }
    }
}

