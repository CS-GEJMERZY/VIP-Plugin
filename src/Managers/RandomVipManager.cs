using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace Core.Managers
{
    public class RandomVipManager
    {
        internal Config.RandomVipConfig RandomVipData { get; set; }

        public RandomVipManager(Config.RandomVipConfig randomVipData)
        {
            RandomVipData = randomVipData;
        }

        public bool IsRound(int RoundNumber)
        {
            return RoundNumber == RandomVipData.AfterRound;
        }

        public void ProcessRound(IStringLocalizer Localizer)
        {
            var players = PlayerManager.GetValidPlayers().Where(
                player => !PermissionManager.HasAnyPermission(player, RandomVipData.PermissionExclude) &&
                           player.Connected == PlayerConnectedState.PlayerConnected).ToList();

            if (players.Count == 0 ||
                players.Count < RandomVipData.MinimumPlayers) { return; }

            var randomPlayer = ChooseRandomPlayer(players);

            AnnouncePickingProcess(Localizer);
            PermissionManager.AddPermissions(randomPlayer, RandomVipData.PermissionsGranted);
            AnnounceWinner(randomPlayer, Localizer);
        }

        public static void AnnounceWinner(CCSPlayerController player, IStringLocalizer Localizer)
        {
            Server.PrintToChatAll(Localizer["winner", player.PlayerName]);
        }

        private void AnnouncePickingProcess(IStringLocalizer Localizer)
        {
            for (int i = 0; i < RandomVipData.RepeatPicking; i++)
            {
                Server.PrintToChatAll(Localizer["picking"]);
            }
        }

        private static CCSPlayerController ChooseRandomPlayer(List<CCSPlayerController> players)
        {
            Random random = new();
            int randomIndex = random.Next(0, players.Count);
            return players[randomIndex];
        }
    }


}

