using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Core;

public partial class Plugin
{
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_vipdebug", "Vip plugin debug command")]
    public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) { return; }

        if (!PlayerCache.ContainsKey(player))
        {
            player!.PrintToChat(Localizer["not_registered"]);
        }
        else
        {
            player!.PrintToChat($"Your group id: {PlayerCache[player].GroupId} | Index: {player.Index} ");
            foreach (var group in Config!.VIPGroups)
            {
                bool hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);

                if (hasPerms)
                {
                    player!.PrintToChat($"{group.Name} {group.Permissions} | has permission");
                }
                else
                {
                    player!.PrintToChat($"{group.Name} {group.Permissions} | no permission");
                }
            }
        }

        player.PrintToChat($"Random VIP Enabled: {Config!.RandomVIP.Enabled}");
        player.PrintToChat($"Round for VIP: {Config!.RandomVIP.AfterRound}");
        player.PrintToChat($"Night VIP enabled: {Config.NightVIP.Enabled}");
        player.PrintToChat($"Night VIP is hour: {NightVipManager!.IsNightVipTime()}");
    }
}