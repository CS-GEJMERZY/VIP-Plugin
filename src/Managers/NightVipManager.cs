using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Plugin.Managers
{
    public class NightVipManager
    {
        private Models.NightVIPData nightVipData { get; set; }

        public NightVipManager(Models.NightVIPData nightVipData)
        {
            this.nightVipData = nightVipData;
        }

        public bool IsNightVipTime()
        {
            if (!nightVipData.Enabled) return false;

            int currentHour = DateTime.Now.Hour;
            if (nightVipData.StartHour <= nightVipData.EndHour)
            {
                // Normal scenario (e.g., 22:00 to 8:00)
                return currentHour >= nightVipData.StartHour || currentHour < nightVipData.EndHour;
            }
            else
            {
                // Overnight scenario (e.g., 8:00 to 22:00)
                return currentHour >= nightVipData.StartHour && currentHour < nightVipData.EndHour;
            }
        }

        public bool PlayerQualifies(CCSPlayerController player)
        {
            if (!nightVipData.Enabled || !IsNightVipTime()) return false;

            if (!HasRequiredPhrase(player)) return false;
            if (!HasRequiredTag(player)) return false;
            if (HasAnyExcludedPermission(player)) return false;

            return true;
        }

        private bool HasRequiredPhrase(CCSPlayerController player)
        {
            if (nightVipData.RequiredNickPhrase == string.Empty) return true;
            return player.PlayerName.Contains(nightVipData.RequiredNickPhrase, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasRequiredTag(CCSPlayerController player)
        {
            if (nightVipData.RequiredScoreboardTag == string.Empty) return true;
            return player.Clan == nightVipData.RequiredScoreboardTag;
        }

        private bool HasAnyExcludedPermission(CCSPlayerController player)
        {
            return nightVipData.PermissionExclude.Any(perm => AdminManager.PlayerHasPermissions(player, perm));
        }

        public void GiveNightVip(CCSPlayerController player)
        {
            PermissionManager.AddPermissions(player, nightVipData.PermissionsGranted);
        }
    }
}

