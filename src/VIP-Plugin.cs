using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Plugin;

public partial class VipPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.0.4";

    public required PluginConfig Config { get; set; }
    internal Managers.GroupManager? groupManager { get; set; }
    internal Managers.RandomVipManager? randomVipManager { get; set; }
    internal Managers.NightVipManager? nightVipManager { get; set; }

    internal Dictionary<CCSPlayerController, Models.PlayerData> PlayerCache = new();

    internal static ILogger? _logger;

    public void OnConfigParsed(PluginConfig _Config)
    {
        Config = _Config;

        groupManager = new Managers.GroupManager(Config.Groups);
        randomVipManager = new Managers.RandomVipManager(Config.RandomVIP);
        nightVipManager = new Managers.NightVipManager(Config.NightVIP);
    }

    public override void Load(bool hotReload)
    {
        _logger = Logger;

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
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

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }
}
