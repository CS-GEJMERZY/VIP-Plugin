using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace Plugin.Managers
{
    public class GroupManager
    {
        private readonly List<Models.VipGroupData> _groups;
        public GroupManager(List<Models.VipGroupData> groups)
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
}


