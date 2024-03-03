using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace VIP;

public class RandomVipManager
{
    private void AnnouncePickingProcess()
    {
        for (int i = 0; i < Config!.RandomVIP.RepeatPicking; i++)
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

