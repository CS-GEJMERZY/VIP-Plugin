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
    public PlayerTestVipData TestVipData { get; set; } = new();
    public DateTime ConnectDate { get; set; } = DateTime.UtcNow;
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

        if (Group == null || baseGroup.Priority > Group.Priority)
        {
            Group = baseGroup;
        }
    }

    public async Task LoadDatabaseVipDataAsync(CCSPlayerController player, GroupManager groupManager, DatabaseManager databaseManager)
    {
        ulong steamId64 = 0;
        string name = string.Empty;

        await Server.NextFrameAsync(() =>
        {
            steamId64 = player!.AuthorizedSteamID!.SteamId64;
            name = player.PlayerName;
        });

        DatabaseData.Id = await databaseManager.GetPlayerId(steamId64, name);
        DatabaseData.Services = await databaseManager.GetPlayerServices(DatabaseData.Id);

        DatabaseData.AllFlags.Clear();
        foreach (var service in DatabaseData.Services)
        {
            if (service.Availability != ServiceAvailability.Enabled)
            {
                continue;
            }

            if (service.Start > DateTime.UtcNow)
            {
                continue;
            }

            if (service.End <= DateTime.UtcNow)
            {
                await databaseManager.SetServiceAvailability(service.Id, ServiceAvailability.Expired);
                continue;
            }

            var databaseGroup = groupManager.GetGroup(service.GroupId);
            if (databaseGroup != null)
            {
                if (Group == null || databaseGroup.Priority > Group.Priority)
                {
                    Group = databaseGroup;
                }
            }

            DatabaseData.AllFlags.UnionWith(service.Flags);
        }

        Server.NextFrame(() =>
        {
            if (!PlayerManager.IsValid(player))
            {
                return;
            }

            PermissionManager.AddPermissions(player, DatabaseData.AllFlags);
        });
    }

    public async Task LoadTestVipDataAsync(CCSPlayerController player, GroupManager groupManager, DatabaseManager databaseManager, TestVipConfig config)
    {
        TestVipData.ActiveTestVip = await databaseManager.GetPlayerTestVipData(DatabaseData.Id);
        TestVipData.LastEndTime = await databaseManager.GetPlayerTestVipLatestUsedDate(DatabaseData.Id);

        if (TestVipData.ActiveTestVip == null ||
            TestVipData.ActiveTestVip.Start > DateTime.UtcNow)
        {
            return;
        }

        switch (TestVipData.ActiveTestVip!.Mode)
        {
            case TestVipMode.FixedDate:
                {
                    if (TestVipData.ActiveTestVip.End <= DateTime.UtcNow)
                    {
                        await databaseManager.UpdateTestVipCompleted(TestVipData.ActiveTestVip.Id, true);
                        TestVipData.LastEndTime = TestVipData.ActiveTestVip.End;
                        TestVipData.ActiveTestVip = null;
                    }

                    break;
                }
            case TestVipMode.Playtime:
                {
                    if (TestVipData.ActiveTestVip.TimeLeft <= 0)
                    {
                        await databaseManager.UpdateTestVipCompleted(TestVipData.ActiveTestVip.Id, true);
                        await databaseManager.UpdateTestVipEndTime(TestVipData.ActiveTestVip.Id, DateTime.UtcNow);
                        TestVipData.LastEndTime = DateTime.UtcNow;
                        TestVipData.ActiveTestVip = null;
                    }

                    break;
                }
        }

        if (TestVipData.ActiveTestVip != null)
        {
            var testVipGroup = groupManager.GetGroup(config.UniqueGroupId);
            if (testVipGroup != null)
            {
                if (Group == null ||
                   testVipGroup.Priority > Group.Priority)
                {
                    Group = testVipGroup;
                }
            }

            await Server.NextFrameAsync(() =>
            {
                PermissionManager.AddPermissions(player, config.PermissionsGranted);
            });
        }
    }
}
