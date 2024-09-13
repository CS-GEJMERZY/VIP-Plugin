using Core.Config;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using Microsoft.Extensions.Localization;

namespace Core.Managers;

public class NightVipManager(NightVipConfig nightVipData, string prefix)
{
    private NightVipConfig NightVipData { get; set; } = nightVipData;
    internal string Prefix { get; set; } = prefix;

    public bool IsNightVipTime()
    {
        string timeZone = NightVipData.TimeZone;
        TimeZoneInfo timeZoneInfo;
        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
        }
      
        if (!NightVipData.Enabled)
        {
            return false;
        }

        DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
        int currentHour = currentTime.Hour;

        // Check if the current time is within the Night VIP time window
        if (NightVipData.StartHour <= NightVipData.EndHour)
        {
            // Normal scenario (e.g., 8:00 to 22:00)
            return currentHour >= NightVipData.StartHour && currentHour < NightVipData.EndHour;
        }
        else
        {
            // Overnight scenario (e.g., 22:00 to 8:00)
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
        return NightVipData.PermissionsExclude.Any(perm => AdminManager.PlayerHasPermissions(player, perm));
    }

    public void GiveNightVip(CCSPlayerController player, IStringLocalizer Localizer)
    {
        PermissionManager.AddPermissions(player, NightVipData.PermissionsGranted);
        if (NightVipData.SendMessageOnVIPReserved)
        {
            player.PrintToCenterAlert($"{Prefix}{Localizer["nightvip.resaved"]}");
        }
    }
}

