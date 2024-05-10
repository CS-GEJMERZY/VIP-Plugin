using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

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
        for (int i = 0; i < count; i++)
        {
            player.GiveNamedItem(item);
        }

        var playerPawn = player!.PlayerPawn.Value;
        Utilities.SetStateChanged(playerPawn!, "CBasePlayerPawn", "m_pItemServices");
    }

    public static void GiveItem(CCSPlayerController player, string item, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            player.GiveNamedItem(item);
        }

        var playerPawn = player!.PlayerPawn.Value;
        Utilities.SetStateChanged(playerPawn!, "CBasePlayerPawn", "m_pItemServices");
    }

    public static int GetHealth(CCSPlayerController player)
    {
        return player!.PlayerPawn!.Value!.Health;
    }

    public static void SetHealth(CCSPlayerController player, int amount, int maxHealth = (int)1e6)
    {
        var playerPawn = player!.PlayerPawn.Value;

        playerPawn!.Health = Math.Min(amount, maxHealth);
        Utilities.SetStateChanged(playerPawn!, "CBaseEntity", "m_iHealth");
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

        var playerPawn = player!.PlayerPawn.Value;
        Utilities.SetStateChanged(playerPawn!, "CCSPlayerController_InGameMoneyServices", "m_iAccount");
    }

    public static int GetArmor(CCSPlayerController player)
    {
        return player!.PlayerPawn!.Value!.ArmorValue;
    }

    public static void SetArmor(CCSPlayerController player, int amount)
    {
        var playerPawn = player!.PlayerPawn.Value;

        playerPawn!.ArmorValue = amount;
        Utilities.SetStateChanged(playerPawn!, "CCSPlayerPawn", "m_ArmorValue");
    }

    public static void AddArmor(CCSPlayerController player, int amount)
    {
        var playerPawn = player!.PlayerPawn.Value;

        int newValue = Math.Max(100, GetArmor(player) + amount);
        playerPawn!.ArmorValue = newValue;

        Utilities.SetStateChanged(playerPawn!, "CCSPlayerPawn", "m_ArmorValue");
    }
}

