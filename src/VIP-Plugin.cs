using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using static CounterStrikeSharp.API.Core.Listeners;

namespace VIP;

public partial class VIPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "0.0.5";

    public PluginConfig Config { get; set; }
    internal VIPGroupManager? GroupManager { get; set; }

    internal Dictionary<CCSPlayerController, VIPPlayer> PlayerCache = new Dictionary<CCSPlayerController, VIPPlayer>();

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        GroupManager = new VIPGroupManager(Config.Groups);
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<OnClientDisconnect>(OnClientDisconnect);

        RegisterListener<Listeners.OnTick>(() =>
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
                if (player != null && player.IsValid && !player.IsBot && !player.IsHLTV)
                {
                    PlayerCache.Add(player, new VIPPlayer());
                    PlayerCache[player].LoadGroup(player, GroupManager!);
                }
            }
        }
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_vipdebug", "VIP PLUGIN DEBUG")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) { return; }

        if (!PlayerCache.ContainsKey(player))
        {
            player!.PrintToChat("You have not been registered yet by the plugin.");
            return;
        }

        player!.PrintToChat($"Your group id: {PlayerCache[player].GroupID}");
        foreach (var group in Config.Groups)
        {
            bool hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);

            if (hasPerms)
            {
                player!.PrintToChat($"{group.Name} {group.Permissions} | HAS");
            }
            else
            {
                player!.PrintToChat($"{group.Name} {group.Permissions} | DOESN'T HAVE");
            }
        }
    }

    public bool IsPistolRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules == null) return false;

        var halftime = ConVar.Find("mp_halftime")!.GetPrimitiveValue<bool>();
        var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();


        return gameRules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gameRules.TotalRoundsPlayed) ||
               gameRules.GameRestart;
    }

}
