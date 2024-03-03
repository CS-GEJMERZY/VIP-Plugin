using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using static CounterStrikeSharp.API.Core.Listeners;
namespace VIP;

public partial class VipPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.0.0";

    public PluginConfig? Config { get; set; }
    internal GroupManager? groupManager { get; set; }

    internal Dictionary<CCSPlayerController, VIPPlayer> PlayerCache = new Dictionary<CCSPlayerController, VIPPlayer>();

    public void OnConfigParsed(PluginConfig _Config)
    {
        Config = _Config;

        groupManager = new GroupManager(Config.Groups);
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
                if (player != null && player.IsValid && !player.IsBot && !player.IsHLTV)
                {
                    PlayerCache.Add(player, new VIPPlayer());
                    PlayerCache[player].LoadGroup(player, groupManager!);
                }
            }
        }
    }





}
