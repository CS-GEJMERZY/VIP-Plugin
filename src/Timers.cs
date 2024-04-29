using Core.Config;
using Core.Managers;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace Core;

public partial class Plugin
{
    public void HealthRegenCallback(object state)
    {
        var group = (VipGroupConfig)state;

        if (group == null)
        {
            Logger.LogError("group is null in HealthRegenCallback");
            return;
        }

        Server.NextFrame(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!player.IsValid ||
                    !player.PawnIsAlive ||
                    !_playerCache.ContainsKey(player))
                {
                    continue;
                }

                var playerData = _playerCache[player];
                if (playerData.Group != group)
                {
                    continue;
                }

                PlayerManager.AddHealth(player, group.Misc.HealthRegen.Amount, group.Limits.MaxHp);
            }
        });
    }

    public void ArmorRegenCallback(object state)
    {
        var group = (VipGroupConfig)state;

        if (group == null)
        {
            Logger.LogError("group is null in ArmorRegenCallback");
            return;
        }

        Server.NextFrame(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!player.IsValid ||
                    !player.PawnIsAlive ||
                    !_playerCache.ContainsKey(player))
                {
                    continue;
                }

                var playerData = _playerCache[player];
                if (playerData.Group != group)
                {
                    continue;
                }

                PlayerManager.AddArmor(player, group.Misc.ArmorRegen.Amount);
            }
        });
    }
}
