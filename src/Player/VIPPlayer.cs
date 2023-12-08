using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class VIPPlayer
{
    public int GroupID { get; set; }

    public PlayerTempData TempData;


    public VIPPlayer()
    {
        this.GroupID = -1;
        TempData = new PlayerTempData();
    }

    public void LoadGroup(CCSPlayerController player, VIPGroupManager groupManager)
    {
        this.GroupID = groupManager.GetPlayerGroup(player);
    }

    public static void RefreshUI(CCSPlayerController player, gear_slot_t slot)
    {
        player.ExecuteClientCommand("lastinv");
        player.ExecuteClientCommand("slot" + slot);
    }

    public static bool IsValid(CCSPlayerController player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }

    public static void AddMoney(CCSPlayerController playerController, int amount)
    {
        throw new NotImplementedException();
    }

    public class PlayerTempData
    {
        public int JumpsUsed { get; set; } = 0;
        public PlayerButtons LastButtons { get; set; }
        public PlayerFlags LastFlags { get; set; }
    }
}