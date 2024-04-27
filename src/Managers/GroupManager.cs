using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Core.Managers;

public class GroupManager(List<Config.VipGroupConfig> groups)
{
    private readonly List<Config.VipGroupConfig> _groups = groups;

    public int GetPlayerGroup(CCSPlayerController player)
    {
        for (int i = 0; i < _groups.Count; i++)
        {
            if (AdminManager.PlayerHasPermissions(player, _groups[i].Permissions))
            {
                return i;
            }
        }

        return -1;
    }
}

