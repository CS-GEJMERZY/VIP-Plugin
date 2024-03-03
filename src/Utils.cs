using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace VIP;

public partial class VIPlugin
{
    public int GetTeamScore(CsTeam team)
    {
        var teamManagers = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        foreach(var manager in teamManagers)
        {
            if((int)team == manager.TeamNum)
            {
                return manager.Score;
            }
        }

        return 0;
    }

    private void AnnouncePickingProcess()
    {
        for (int i = 0; i < Config.RandomVIP.repeatPicking; i++)
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

    private List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().Where(IsPlayerValid).ToList();
    }

    public bool IsPlayerValid(CCSPlayerController player)
    {
        return !(player == null || !player.IsValid || !player.PlayerPawn.IsValid || player.IsBot);
    }

    public void GivePlayerRandomVIP(CCSPlayerController player)
    {
        foreach(var permission in Config.RandomVIP.permissions)
        {
            AdminManager.AddPlayerPermissions(player, permission);
        }
    }
}