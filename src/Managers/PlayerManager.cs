using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace Core.Managers
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

        //public static void StripGrenades(CCSPlayerController player)
        //{
        //    if (player!.PlayerPawn!.Value!.WeaponServices == null) return;

        //    var grenades = new[] { "weapon_smokegrenade", "weapon_hegrenade", "weapon_flashbang", "weapon_molotov", "weapon_incgrenade" };

        //    foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
        //    {
        //        if (weapon != null &&
        //            weapon!.IsValid &&
        //            weapon!.Value!.IsValid &&
        //            grenades.Contains(weapon.Value.DesignerName))
        //        {
        //            //player.RemoveItemByDesignerName(weapon.Value.DesignerName, false);
        //        }
        //    }
        //}

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
            player!.PlayerPawn!.Value!.Health = Math.Min(amount, maxHealth);

            var playerPawn = player!.PlayerPawn.Value;
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

        public static void SetArmor(CCSPlayerController player, int amount)
        {
            player!.PlayerPawn!.Value!.ArmorValue = amount;

            var playerPawn = player!.PlayerPawn.Value;
            Utilities.SetStateChanged(playerPawn!, "CCSPlayerPawnBase", "m_ArmorValue");
        }
    }
}




