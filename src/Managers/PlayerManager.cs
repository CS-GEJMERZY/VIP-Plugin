using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core.Managers;

public class PlayerManager
{
    public static bool IsValid(CCSPlayerController? player)
    {
        return player != null &&
               player.IsValid &&
               player.PlayerPawn.IsValid &&
               player.AuthorizedSteamID != null;
    }

    public static List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().Where(IsValid).ToList();
    }

    public static void GiveItem(CCSPlayerController player, CsItem item, int count = 1)
    {
        string? enumMemberAttributeValue = EnumUtils.GetEnumMemberAttributeValue(item);
        if (string.IsNullOrWhiteSpace(enumMemberAttributeValue))
        {
            return;
        }

        GiveItem(player, enumMemberAttributeValue, count);
    }

    public static void GiveItem(CCSPlayerController player, string item, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            player.GiveNamedItem(item);
        }

        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pItemServices");
    }

    public static bool HasWeapon(CCSPlayerController player, string weaponName)
    {
        CCSPlayerPawn playerPawn = player.PlayerPawn!.Value!;

        return HasWeapon(playerPawn, weaponName);
    }

    public static bool HasWeapon(CCSPlayerPawn playerPawn, string weaponName)
    {
        foreach (var weapon in playerPawn.WeaponServices!.MyWeapons)
        {
            if (weapon.IsValid &&
                weapon!.Value!.IsValid &&
                weapon.Value.DesignerName == weaponName)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetHealth(CCSPlayerController player)
    {
        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        return playerPawn.Health;
    }

    public static void SetHealth(CCSPlayerController player, int amount, int maxHealth = (int)1e6)
    {
        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        playerPawn!.Health = Math.Min(amount, maxHealth);
        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
    }

    public static void AddHealth(CCSPlayerController player, int amount, int maxHealth = (int)1e6)
    {
        int newHealth = GetHealth(player) + amount;
        SetHealth(player, newHealth, maxHealth);
    }

    public static int GetMoney(CCSPlayerController player)
    {
        return player!.InGameMoneyServices!.Account;
    }

    public static void AddMoney(CCSPlayerController player, int amount, int maxMoney)
    {
        player!.InGameMoneyServices!.Account = Math.Min(GetMoney(player) + amount, maxMoney);

        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        Utilities.SetStateChanged(playerPawn, "CCSPlayerController", "m_pInGameMoneyServices");
    }

    public static int GetArmor(CCSPlayerController player)
    {
        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        return playerPawn.ArmorValue;
    }

    public static void SetArmor(CCSPlayerController player, int amount)
    {
        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        playerPawn.ArmorValue = amount;

        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_ArmorValue");
    }

    public static void AddArmor(CCSPlayerController player, int amount)
    {
        CCSPlayerPawn playerPawn = player!.PlayerPawn!.Value!;

        int newValue = Math.Max(100, GetArmor(player) + amount);
        playerPawn!.ArmorValue = newValue;

        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_ArmorValue");
    }
}

