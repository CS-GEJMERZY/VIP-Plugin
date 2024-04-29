using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core;

public partial class Plugin
{
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_vipdebug", "Vip plugin debug command")]
    public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null)
        {
            return;
        }

        player.PrintToChat($"{ChatColors.Red}[VIP-Plugin] Debug info has been printed in the console.");

        player.PrintToConsole("---Group data ---");
        if (!_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            player.PrintToConsole("* Your group is null");
        }
        else
        {
            var pGroup = playerData.Group!;
            player.PrintToConsole($"Name: {pGroup.Name}");
            player.PrintToConsole($"Priority: {pGroup.Priority}");
            player.PrintToConsole($"UniqueId: {pGroup.UniqueId}");
            player.PrintToConsole($"Permissions: {pGroup.Permissions}");
            player.PrintToConsole($"---All group perms---");

            foreach (var group in Config!.VIPGroups)
            {
                var hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);
                player.PrintToChat($"{group.Name}-{group.Permissions}-{(hasPerms ? "yes" : "no")}");
            }
        }

        player.PrintToConsole($"---RandomVIP---");

        player.PrintToConsole($"Enabled: {Config.RandomVip.Enabled}");
        player.PrintToConsole($"AfterRound: {Config.RandomVip.AfterRound}");
        player.PrintToConsole($"Minimum players: {Config.RandomVip.MinimumPlayers}");
        player.PrintToConsole($"RepeatPicking: {Config.RandomVip.RepeatPickingMessage}");
        player.PrintToConsole($"PermissionsGranted: {string.Join(", ", Config.RandomVip.PermissionsGranted)}");
        player.PrintToConsole($"PermissionExclude: {string.Join(", ", Config.RandomVip.PermissionsExclude)}");

        player.PrintToConsole($"---NightVIP---");

        player.PrintToConsole($"Enabled: {Config.NightVip.Enabled}");
        player.PrintToConsole($"is time: {NightVipManager!.IsNightVipTime()}");
        player.PrintToConsole($"StartHour: {Config.NightVip.StartHour}");
        player.PrintToConsole($"EndHour: {Config.NightVip.EndHour}");
        player.PrintToConsole($"RequiredNickPhrase: {Config.NightVip.RequiredNickPhrase}");
        player.PrintToConsole($"RequiredScoreboardTag: {Config.NightVip.RequiredScoreboardTag}");
        player.PrintToConsole($"PermissionsGranted: {string.Join(", ", Config.NightVip.PermissionsGranted)}");
        player.PrintToConsole($"PermissionExclude: {string.Join(", ", Config.NightVip.PermissionsExclude)}");
    }
}
