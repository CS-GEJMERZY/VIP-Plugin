using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Plugin.Managers
{
    public class PlayerManager
    {
        public static bool IsValid(CCSPlayerController player)
        {
            return player != null &&
                   player.IsValid &&
                   player.PlayerPawn.IsValid;
        }

        public static List<CCSPlayerController> GetValidPlayers()
        {
            return Utilities.GetPlayers().Where(IsValid).ToList();
        }

        public static int GetHealth(CCSPlayerController player)
        {
            return player!.PlayerPawn!.Value!.Health;
        }

        public static void SetHealth(CCSPlayerController player, int amount)
        {
            player!.PlayerPawn!.Value!.Health = amount;
        }

        public static void AddHealth(CCSPlayerController player, int amount)

        {
            int newHealth = GetHealth(player) + amount;
            SetHealth(player, newHealth);
        }

        public static void AddMoney(CCSPlayerController player, int amount)
        {
            player!.InGameMoneyServices!.Account += amount;
        }

        public static void SetArmor(CCSPlayerController player, int amount)
        {
            player!.PlayerPawn!.Value!.ArmorValue = amount;
        }
    }
}


