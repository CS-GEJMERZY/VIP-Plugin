using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace Core.Managers;

public class RandomVipManager
{
    internal Config.RandomVipConfig RandomVipData { get; set; }

    internal string Prefix { get; set; }

    public RandomVipManager(Config.RandomVipConfig randomVipData, string prefix)
    {
        RandomVipData = randomVipData;
        Prefix = prefix;
    }

    public bool IsRound(int RoundNumber)
    {
        return RoundNumber == RandomVipData.AfterRound;
    }

    public void ProcessRound(IStringLocalizer Localizer)
    {
        var players = PlayerManager.GetValidPlayers().Where(
            player => !PermissionManager.HasAnyPermission(player, RandomVipData.PermissionExclude) &&
                       player.Connected == PlayerConnectedState.PlayerConnected &&
                       !string.IsNullOrEmpty(player.IpAddress)).ToList();

        if (players.Count == 0 ||
            players.Count < RandomVipData.MinimumPlayers) { return; }

        var randomPlayer = ChooseRandomPlayer(players);

        AnnouncePickingProcess(Localizer);
        PermissionManager.AddPermissions(randomPlayer, RandomVipData.PermissionsGranted);
        AnnounceWinner(randomPlayer, Localizer);
    }

    public void AnnounceWinner(CCSPlayerController player, IStringLocalizer Localizer)
    {
        Server.PrintToChatAll($" {Prefix}{Localizer["winner", player.PlayerName]}");
    }

    private void AnnouncePickingProcess(IStringLocalizer Localizer)
    {
        for (int i = 0; i < RandomVipData.RepeatPickingMessage; i++)
        {
            Server.PrintToChatAll($" {Prefix}{Localizer["picking"]}");
        }
    }

    private static CCSPlayerController ChooseRandomPlayer(List<CCSPlayerController> players)
    {
        Random random = new();
        int randomIndex = random.Next(0, players.Count);
        return players[randomIndex];
    }
}

