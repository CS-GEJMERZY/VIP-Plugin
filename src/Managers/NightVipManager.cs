using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Plugin.Managers
{
    public class NightVipManager
    {
        private Models.NightVIPData NightVipData { get; set; }

        public NightVipManager(Models.NightVIPData nightVipData)
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
            if (!NightVipData.Enabled || !IsNightVipTime()) return false;

            if (!HasRequiredPhrase(player)) return false;
            return !HasRequiredTag(player) ? false : !HasAnyExcludedPermission(player);
        }

        private bool HasRequiredPhrase(CCSPlayerController player)
        {
            return NightVipData.RequiredNickPhrase == string.Empty
                ? true
                : player.PlayerName.Contains(NightVipData.RequiredNickPhrase, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasRequiredTag(CCSPlayerController player)
        {
            return NightVipData.RequiredScoreboardTag == string.Empty ? true : player.Clan == NightVipData.RequiredScoreboardTag;
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

