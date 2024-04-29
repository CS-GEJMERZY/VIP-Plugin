using Core.Config;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core.Models;

public class PlayerData
{
    public VipGroupConfig? Group { get; set; } = null;
    public PlayerDatabaseData DatabaseData { get; set; } = new();
    public List<string> databaseFlags = [];
    public int JumpsUsed { get; set; } = 0;
    public PlayerButtons LastButtons { get; set; }
    public PlayerFlags LastFlags { get; set; }

    public bool UsingExtraJump { get; set; }

    //public async Task LoadData(CCSPlayerController player, GroupManager groupManager, DatabaseManager databaseManager)
    //{
    //    ulong steamId64 = 0;
    //    string name = string.Empty;

    //    List<int> allGroups = [];
    //    await Server.NextFrameAsync(() =>
    //    {
    //        int baseGroup = groupManager.GetPlayerBaseGroup(player);
    //        if (baseGroup != -1)
    //        {
    //            allGroups.Add(baseGroup);
    //        }

    //        steamId64 = player.AuthorizedSteamID.SteamId64;
    //        name = player.PlayerName;
    //    });

    //    DatabaseData.Id = await databaseManager.GetPlayerData(steamId64, name);
    //    DatabaseData.Services = await databaseManager.GetPlayerServices(DatabaseData.Id);
    //}
}
