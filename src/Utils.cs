using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace VIP;

public partial class VipPlugin
{
    public int GetTeamScore(CsTeam team)
    {
        var teamManagers = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        foreach (var manager in teamManagers)
        {
            if ((int)team == manager.TeamNum)
            {
                return manager.Score;
            }
        }

        return 0;
    }

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


    public bool IsPistolRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules == null) return false;

        var halftime = ConVar.Find("mp_halftime")!.GetPrimitiveValue<bool>();
        var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();


        return gameRules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gameRules.TotalRoundsPlayed) ||
               gameRules.GameRestart;
    }
}