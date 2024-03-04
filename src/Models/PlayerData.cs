using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace Plugin.Models
{
    public class PlayerData
    {
        public int GroupId { get; set; }

        public int JumpsUsed { get; set; } = 0;
        public PlayerButtons LastButtons { get; set; }
        public PlayerFlags LastFlags { get; set; }
        public PlayerData()
        {
            GroupId = -1;
        }
    }
}
