using Core.Managers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core.Models;

public class PlayerData
{
    public int GroupId { get; set; }
    public int JumpsUsed { get; set; } = 0;
    public PlayerButtons LastButtons { get; set; }
    public PlayerFlags LastFlags { get; set; }

    public bool UsingExtraJump { get; set; }
    public PlayerData()
    {
        GroupId = -1;
    }

    public void LoadGroup(GroupManager groupManager, DatabaseManager databaseManager)
    {

    }
}
