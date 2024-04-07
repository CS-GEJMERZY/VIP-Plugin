using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Core
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "VIP Plugin";
        public override string ModuleAuthor => "Hacker";
        public override string ModuleVersion => "1.0.24";
        public required PluginConfig Config { get; set; }

        private GroupManager? GroupManager { get; set; }
        private RandomVipManager? RandomVipManager { get; set; }
        private NightVipManager? NightVipManager { get; set; }

        private readonly Dictionary<CCSPlayerController, PlayerData> _playerCache = [];

        private List<Timer?> HealthRegenTimers { get; set; } = [];
        private List<Timer?> ArmorRegenTimers { get; set; } = [];

        public void OnConfigParsed(PluginConfig _Config)
        {
            Config = _Config;
            string Prefix = $" {MessageFormatter.FormatColor(Config.Settings.Prefix)}";

            GroupManager = new GroupManager(Config.VIPGroups);
            RandomVipManager = new RandomVipManager(Config.RandomVip, Prefix);
            NightVipManager = new NightVipManager(Config.NightVip);

            foreach (var _ in Config.VIPGroups)
            {
                HealthRegenTimers.Add(null);
                ArmorRegenTimers.Add(null);
            }
        }

        public override void Load(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

            RegisterListener<OnTick>(() =>
            {
                foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.PawnIsAlive))
                {
                    if (!_playerCache.ContainsKey(player))
                    {
                        continue;
                    }

                    OnTick(player);
                }
            });

            RegisterListener<OnEntitySpawned>(OnEntitySpawned);

            if (hotReload)
            {
                foreach (var player in Utilities.GetPlayers()
                    .Where(p => PlayerManager.IsValid(p) && !p.IsHLTV))
                {

                    _playerCache.Add(player, new PlayerData { GroupId = GroupManager!.GetPlayerGroup(player) });
                }
            }
        }

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
                    if (playerData.GroupId == -1 || Config.VIPGroups[playerData.GroupId] != group)
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
                    if (playerData.GroupId == -1 || Config.VIPGroups[playerData.GroupId] != group)
                    {
                        continue;
                    }

                    PlayerManager.AddArmor(player, group.Misc.ArmorRegen.Amount);
                }
            });
        }
    }
}

