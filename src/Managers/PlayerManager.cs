using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace VIP;

public class PlayerManager
{
    public static bool IsValid(CCSPlayerController player)
    {
        return !(player == null || !player.IsValid || !player.PlayerPawn.IsValid);
    }

    public static List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().Where(IsValid).ToList();
    }

    public static void RefreshUI(CCSPlayerController player, gear_slot_t slot)
    {
        player.ExecuteClientCommand("lastinv");
        player.ExecuteClientCommand("slot" + slot);
    }

    public static void AddMoney(CCSPlayerController playerController, int amount)
    {
        throw new NotImplementedException();
    }

    public static int GetPlayerGroup(CCSPlayerController player, GroupManager groupManager)
    {
        return  groupManager.GetPlayerGroup(player);
    }
}
