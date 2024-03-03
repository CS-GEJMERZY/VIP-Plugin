using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

public class GroupManager
{
    private readonly List<VIPGroup> _groups;
    public GroupManager(List<VIPGroup> groups)
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
