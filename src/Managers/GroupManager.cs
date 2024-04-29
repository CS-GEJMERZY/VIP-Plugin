using Core.Config;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Core.Managers;

public class GroupManager
{
    private readonly List<VipGroupConfig> _groups = [];

    public GroupManager(List<VipGroupConfig> groups)
    {
        _groups = groups;
        _groups.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public VipGroupConfig? GetPlayerBaseGroup(CCSPlayerController player)
    {
        foreach (var group in _groups)
        {
            if (AdminManager.PlayerHasPermissions(player, group.Permissions))
            {
                return group;
            }
        }

        return null;
    }

    public VipGroupConfig? GetGroup(string id)
    {
        return _groups.Find(x => x.UniqueId == id);
    }
}

