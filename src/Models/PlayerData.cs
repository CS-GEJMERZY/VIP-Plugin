using Core.Config;
using Core.Managers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core.Models;

public class PlayerData
{
    public VipGroupConfig? Group { get; set; } = null;
    public PlayerDatabaseData DatabaseData { get; set; } = new();
    public int JumpsUsed { get; set; } = 0;
    public PlayerButtons LastButtons { get; set; }
    public PlayerFlags LastFlags { get; set; }

    public bool UsingExtraJump { get; set; }

    public void LoadBaseGroup(CCSPlayerController player, GroupManager groupManager)
    {
        var baseGroup = groupManager.GetPlayerBaseGroup(player);
        if (baseGroup == null)
        {
            return;
        }

        if (Group == null)
        {
            Group = baseGroup;
        }
        else if (baseGroup.Priority > Group.Priority)
        {
            Group = baseGroup;
        }
    }
    public async Task LoadData(CCSPlayerController player, GroupManager groupManager, DatabaseManager? databaseManager)
    {
        ulong steamId64 = 0;
        string name = string.Empty;

        SortedSet<VipGroupConfig> allGroups = new(
            Comparer<VipGroupConfig>.Create((a, b) => b.Priority.CompareTo(a.Priority))
        );

        await Server.NextFrameAsync(() =>
        {
            VipGroupConfig? baseGroup = groupManager.GetPlayerBaseGroup(player);
            if (baseGroup != null)
            {
                allGroups.Add(baseGroup);
            }

            steamId64 = player!.AuthorizedSteamID!.SteamId64;
            name = player.PlayerName;
        });

        if (databaseManager != null)
        {
            DatabaseData.Id = await databaseManager.GetPlayerId(steamId64, name);
            DatabaseData.Services = await databaseManager.GetPlayerServices(DatabaseData.Id);

            DatabaseData.AllFlags.Clear();
            foreach (var service in DatabaseData.Services)
            {
                if (service.Availability != ServiceAvailability.Enabled)
                {
                    return;
                }

                if (service.End <= DateTime.UtcNow)
                {
                    await databaseManager.SetServiceAvailability(service.Id, ServiceAvailability.Expired);
                    continue;
                }

                var group = groupManager.GetGroup(service.GroupId);
                if (group != null)
                {
                    allGroups.Add(group);
                }

                DatabaseData.AllFlags.UnionWith(service.Flags);
            }
        }

        if (allGroups.Count > 0)
        {
            Group = allGroups.First();
        }
    }
}
