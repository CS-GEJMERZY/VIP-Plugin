using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core;

public partial class Plugin
{
    public static int GetTeamScore(CsTeam team)
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

    public static bool IsPistolRound()
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules == null) return false;

        var halftime = ConVar.Find("mp_halftime")!.GetPrimitiveValue<bool>();
        var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();


        return gameRules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gameRules.TotalRoundsPlayed) ||
               gameRules.GameRestart;
    }

    public static Color HexToRgb(string hex)
    {
        if (hex[0] == '#')
            hex = hex[1..];

        if (hex.Length != 6)
            throw new ArgumentException("Hexadecimal color string must be exactly 6 characters long.");

        int red = Convert.ToInt32(hex.Substring(0, 2), 16);
        int green = Convert.ToInt32(hex.Substring(2, 2), 16);
        int blue = Convert.ToInt32(hex.Substring(4, 2), 16);

        return Color.FromArgb(red, green, blue);
    }
}