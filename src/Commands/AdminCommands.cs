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

        if (!_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            player!.PrintToChat(Localizer["not_registered"]);
        }
        else
        {
            player!.PrintToChat($"Your group id: {playerData.GroupId}");
            foreach (var group in Config!.VIPGroups)
            {
                var hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);
                player.PrintToChat($"{group.Name} {group.Permissions} | {(hasPerms ? "has permission" : "no permission")}");
            }
        }

        player.PrintToChat($"Random VIP Enabled: {Config.RandomVip.Enabled}");
        player.PrintToChat($"After round VIP: {Config.RandomVip.AfterRound}");
        player.PrintToChat($"Minimum players: {Config.RandomVip.MinimumPlayers}");
        player.PrintToChat($"RepeatPicking: {Config.RandomVip.RepeatPickingMessage}");
        player.PrintToChat($"PermissionsGranted: {Config.RandomVip.PermissionsGranted}");
        player.PrintToChat($"PermissionExclude: {Config.RandomVip.PermissionExclude}");

        player.PrintToChat($"Night VIP enabled: {Config.NightVip.Enabled}");
        player.PrintToChat($"Night VIP is hour: {NightVipManager!.IsNightVipTime()}");
        player.PrintToChat($"StartHour: {Config.NightVip.StartHour}");
        player.PrintToChat($"EndHour: {Config.NightVip.EndHour}");
        player.PrintToChat($"RequiredNickPhrase: {Config.NightVip.RequiredNickPhrase}");
        player.PrintToChat($"RequiredScoreboardTag: {Config.NightVip.RequiredScoreboardTag}");
        player.PrintToChat($"PermissionsGranted: {Config.NightVip.PermissionsGranted}");
        player.PrintToChat($"PermissionExclude: {Config.NightVip.PermissionExclude}");
    }
}