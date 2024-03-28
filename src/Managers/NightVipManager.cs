using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Core.Managers
{
    public class NightVipManager
    {
        private Config.NightVipConfig NightVipData { get; set; }

        public NightVipManager(Config.NightVipConfig nightVipData)
        {
            this.NightVipData = nightVipData;
        }

        public bool IsNightVipTime()
        {
            if (!NightVipData.Enabled) return false;

            int currentHour = DateTime.Now.Hour;
            if (NightVipData.StartHour <= NightVipData.EndHour)
            {
                // Overnight scenario (e.g., 8:00 to 22:00)
                return currentHour >= NightVipData.StartHour && currentHour < NightVipData.EndHour;
            }
            else
            {
                // Normal scenario (e.g., 22:00 to 8:00)
                return currentHour >= NightVipData.StartHour || currentHour < NightVipData.EndHour;
            }
        }

        public bool PlayerQualifies(CCSPlayerController player)
        {
            return NightVipData.Enabled &&
                IsNightVipTime() &&
                HasRequiredPhrase(player) &&
                HasRequiredTag(player) &&
                !HasAnyExcludedPermission(player);
        }

        private bool HasRequiredPhrase(CCSPlayerController player)
        {
            return NightVipData.RequiredNickPhrase == string.Empty ||
                player.PlayerName.Contains(NightVipData.RequiredNickPhrase, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasRequiredTag(CCSPlayerController player)
        {
            return NightVipData.RequiredScoreboardTag == string.Empty ||
                player.Clan == NightVipData.RequiredScoreboardTag;
        }

        private bool HasAnyExcludedPermission(CCSPlayerController player)
        {
            return NightVipData.PermissionExclude.Any(perm => AdminManager.PlayerHasPermissions(player, perm));
        }

        public void GiveNightVip(CCSPlayerController player)
        {
            PermissionManager.AddPermissions(player, NightVipData.PermissionsGranted);
        }
    }


}

