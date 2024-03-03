using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace VIP;
public class GroupManager
{
    private readonly List<VipGroupData> _groups;
    public GroupManager(List<VipGroupData> groups)
    {
        _groups = groups;
    }

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
