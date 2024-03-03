using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace Plugin.Managers
{
    public class RandomVipManager
    {
        internal Models.RandomVIPData randomVipData { get; set; }

        public RandomVipManager(Models.RandomVIPData randomVipData)
        {
            this.randomVipData = randomVipData;
        }

        public bool IsRound(int RoundNumber)
        {
            return RoundNumber == randomVipData.AfterRound;
        }

        public void ProcessRound(IStringLocalizer Localizer)
        {
            var players = PlayerManager.GetValidPlayers();
            if (players.Count == 0) { return; }

            var randomPlayer = ChooseRandomPlayer(players);

            AnnouncePickingProcess(Localizer);
            PermissionManager.AddPermissions(randomPlayer, randomVipData.PermissionsGranted);
            AnnounceWinner(randomPlayer, Localizer);

        }

        public void AnnounceWinner(CCSPlayerController player, IStringLocalizer Localizer)
        {
            Server.PrintToChatAll(Localizer["winner", player.PlayerName]);
        }

        private void AnnouncePickingProcess(IStringLocalizer Localizer)
        {
            for (int i = 0; i < randomVipData.RepeatPicking; i++)
            {
                Server.PrintToChatAll(Localizer["picking"]);
            }
        }


        private CCSPlayerController ChooseRandomPlayer(List<CCSPlayerController> players)
        {
            Random random = new Random();
            int randomIndex = random.Next(0, players.Count);
            return players[randomIndex];
        }
    }


}

