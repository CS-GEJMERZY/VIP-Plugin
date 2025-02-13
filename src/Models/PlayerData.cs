using Core.Config;
using Core.Managers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
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
        Group = null;

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
        bool valid = true;

        await Server.NextFrameAsync(() =>
        {
            if (!PlayerManager.IsValid(player))
            {
                valid = false;
                return;
            }
            steamId64 = player.AuthorizedSteamID!.SteamId64;
            name = player.PlayerName;
        });

        if (!valid) return;

        var oldGroup = Group;

        Group = null;

        await RemoveOldGroupPermissions(player, oldGroup);

        if (databaseManager != null)
        {
            await LoadDatabaseData(steamId64, name, groupManager, databaseManager);
        }

        ApplyGroupPermissions();

        if (DatabaseData.AllFlags.Any())
        {
            await AddNewPermissions(player);
        }
        //if no group is loaded from the database
        //load base group, that is based on use permissions.
        if (Group == null)
            Server.NextWorldUpdate(()=>{
                this.LoadBaseGroup(player,groupManager);
            });
    }

    private async Task RemoveOldGroupPermissions(CCSPlayerController player, VipGroupConfig? oldGroup)
    {
        await Server.NextWorldUpdateAsync(() =>
        {
            if (oldGroup != null && !string.IsNullOrEmpty(oldGroup.Permissions))
            {
                AdminData adminData = AdminManager.GetPlayerAdminData(player)!;

                if (adminData != null && adminData.Flags.Any())
                {
                 
                    var flagsToRemove = new HashSet<string>();

                    foreach (var flagSet in adminData.Flags.Values)
                    {
                        if (flagSet.Contains(oldGroup.Permissions))
                        {
                            flagsToRemove.Add(oldGroup.Permissions);
                        }
                    }

                    if (flagsToRemove.Any())
                    {
                        adminData.RemoveFlags(flagsToRemove);
                    }
                }
            }
        });
    }


    private async Task LoadDatabaseData(ulong steamId64, string name, GroupManager groupManager, DatabaseManager databaseManager)
    {
        
        var oldFlags = new HashSet<string>(DatabaseData.AllFlags.ToList());
    
        // Clear current flags to reload fresh data
        DatabaseData.AllFlags.Clear();


        DatabaseData.Id = await databaseManager.GetPlayerId(steamId64, name);
        DatabaseData.Services = await databaseManager.GetPlayerServices(DatabaseData.Id);

        SortedSet<VipGroupConfig> allGroups = new(
            Comparer<VipGroupConfig>.Create((a, b) => b.Priority.CompareTo(a.Priority))
        );

    
        foreach (var service in DatabaseData.Services)
        {
            if (service.Availability != ServiceAvailability.Enabled)
            {
                return;
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

            var group = groupManager.GetGroup(service.GroupId);
            if (group != null)
            {
                allGroups.Add(group);
            }

            DatabaseData.AllFlags.UnionWith(service.Flags);
        }
        if (allGroups.Count > 0)
        {
            Group = allGroups.First();
        }
        // Compare old and new flags to find obsolete ones
        await RemoveObsoleteFlags(steamId64, oldFlags, DatabaseData.AllFlags);
    }

    private async Task RemoveObsoleteFlags(ulong steamId64, HashSet<string> oldFlags, HashSet<string> newFlags)
    {
        await Server.NextWorldUpdateAsync(() =>
        {
            var player = Utilities.GetPlayers().FirstOrDefault(p =>
                p != null &&
                p.AuthorizedSteamID != null &&
                p.AuthorizedSteamID.SteamId64 == steamId64
            );

            if (player == null) return;

            AdminData adminData = AdminManager.GetPlayerAdminData(player)!;
            if (adminData == null) return;

         
            // Normalize flags before comparison
            var normalizedOldFlags = NormalizeFlags(oldFlags);
            var normalizedNewFlags = NormalizeFlags(newFlags);

            var flagsToRemove = normalizedOldFlags.Except(normalizedNewFlags).ToHashSet();

            if (flagsToRemove.Any())
            {
                adminData.RemoveFlags(flagsToRemove);
            }
        });
    }



    private async Task AddNewPermissions(CCSPlayerController player)
    {
        await Server.NextWorldUpdateAsync(() =>
        {
            var normalizedFlags = NormalizeFlags(DatabaseData.AllFlags);
            PermissionManager.AddPermissions(player, normalizedFlags);
        });
    }

    private void ApplyGroupPermissions()
    {
        if (Group != null && !string.IsNullOrEmpty(Group.Permissions))
        {
            DatabaseData.AllFlags.Add(Group.Permissions);
        }
    }


    private HashSet<string> NormalizeFlags(HashSet<string> flags)
    {
        return flags.Select(f => f.Trim().ToLowerInvariant()).ToHashSet();
    }


}
