using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace VIP;

public partial class VIPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "0.0.2";

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
        RegisterListener<Listeners.OnClientDisconnect>((slot) => { OnClientDisconnect(slot); });
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
            commandInfo.ReplyToCommand("You have not been registered yet by the plugin.");
            return;
        }

        commandInfo.ReplyToCommand($"Your group id: {PlayerCache[player].GroupID}");
        foreach (var group in Config.Groups)
        {
            bool hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);

            if (hasPerms)
            {
                commandInfo.ReplyToCommand($"{group.Name} {group.Permissions} | HAS");
            }
            else
            {
                commandInfo.ReplyToCommand($"{group.Name} {group.Permissions} | DOESN'T HAVE");
            }
        }
    }
}