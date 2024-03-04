using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Plugin;

public partial class VipPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.0.0";

    public PluginConfig? Config { get; set; }
    internal Managers.GroupManager? groupManager { get; set; }
    internal Managers.RandomVipManager? randomVipManager { get; set; }
    internal Managers.NightVipManager? nightVipManager { get; set; }

    internal Dictionary<CCSPlayerController, Models.PlayerData> PlayerCache = new Dictionary<CCSPlayerController, Models.PlayerData>();

    public void OnConfigParsed(PluginConfig _Config)
    {
        Config = _Config;

        groupManager = new Managers.GroupManager(Config.Groups);
        randomVipManager = new Managers.RandomVipManager(Config.RandomVIP);
        nightVipManager = new Managers.NightVipManager(Config.NightVIP);
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<OnClientDisconnect>(OnClientDisconnect);

        RegisterListener<OnTick>(() =>
        {
            foreach (var player in Utilities.GetPlayers()
                         .Where(player => player is { IsValid: true, IsBot: false, PawnIsAlive: true }))
            {
                if (!PlayerCache.ContainsKey(player)) continue;

                OnTick(player);
            }
        });

        if (hotReload)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (Managers.PlayerManager.IsValid(player) && !player.IsHLTV)
                {
                    PlayerCache.Add(player, new Models.PlayerData());
                    PlayerCache[player].GroupId = groupManager!.GetPlayerGroup(player);
                }
            }
        }
    }
}
