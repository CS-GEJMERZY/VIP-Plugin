using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Plugin.Managers
{
    public class PermissionManager
    {
        public static void AddPermissions(CCSPlayerController player, List<string> permissions)
        {
            foreach (var perm in permissions)
            {
                AdminManager.AddPlayerPermissions(player, perm);
            }
        }

        public static bool HasAnyPermission(CCSPlayerController player, List<string> permissions)
        {
            foreach (var perm in permissions)
            {
                if (AdminManager.PlayerHasPermissions(player, perm)) return true;
            }

            return false;
        }
    }
}

