using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class PlayerData
{
    public int GroupID { get; set; }

    public int JumpsUsed { get; set; } = 0;
    public PlayerButtons LastButtons { get; set; }
    public PlayerFlags LastFlags { get; set; }
    public PlayerData()
    {
        this.GroupID = -1;
    }
}