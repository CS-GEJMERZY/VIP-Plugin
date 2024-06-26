﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Core.Managers;

public class PermissionManager
{
    public static void AddPermissions(CCSPlayerController player, IEnumerable<string> permissions)
    {
        foreach (var perm in permissions)
        {
            AdminManager.AddPlayerPermissions(player, perm);
        }
    }
    public static bool HasAnyPermission(CCSPlayerController player, IEnumerable<string> permissions)
    {
        return permissions.Any(perm => AdminManager.PlayerHasPermissions(player, perm));
    }
}

