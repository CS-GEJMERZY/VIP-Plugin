﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
namespace Plugin;

public partial class VipPlugin
{
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_vipdebug", "Vip plugin debug command")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) { return; }

        if (PlayerCache.ContainsKey(player))
        {
            player!.PrintToChat(Localizer["not_registered"]);
            return;
        }

        player!.PrintToChat($"Your group id: {PlayerCache[player].GroupId}");
        foreach (var group in Config!.Groups)
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
}