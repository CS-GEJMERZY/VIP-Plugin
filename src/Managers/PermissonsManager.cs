using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace VIP;

public class PermissionManager
{
    public static void AddPermissions(CCSPlayerController player, List<string> permissions)
    {
        foreach (var perm in permissions)
        {
            AdminManager.AddPlayerPermissions(player, perm);
        }
    }
}