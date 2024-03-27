using Core.Config;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Core;

public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.0.12";

    public required PluginConfig Config { get; set; }

    internal Managers.GroupManager? GroupManager { get; set; }

    internal Managers.RandomVipManager? RandomVipManager { get; set; }

    internal Managers.NightVipManager? NightVipManager { get; set; }

    internal Dictionary<CCSPlayerController, Models.PlayerData> PlayerCache = new();

    public void OnConfigParsed(PluginConfig _Config)
    {
        Config = _Config;

        GroupManager = new Managers.GroupManager(Config.VIPGroups);
        RandomVipManager = new Managers.RandomVipManager(Config.RandomVIP);
        NightVipManager = new Managers.NightVipManager(Config.NightVIP);
    }

    public override void Load(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

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
                    PlayerCache[player].GroupId = GroupManager!.GetPlayerGroup(player);
                }
            }
        }
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }
}
