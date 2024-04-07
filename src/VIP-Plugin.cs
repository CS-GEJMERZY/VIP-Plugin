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
        public override string ModuleVersion => "1.0.21";

        public required PluginConfig Config { get; set; }

        private Managers.GroupManager? GroupManager { get; set; }
        private Managers.RandomVipManager? RandomVipManager { get; set; }
        private Managers.NightVipManager? NightVipManager { get; set; }

        private readonly Dictionary<CCSPlayerController, PlayerData> _playerCache = [];

        private List<Timer?> ArmorRegenTimers { get; set; } = [];

        public void OnConfigParsed(PluginConfig _Config)
        {
            Config = _Config;
            string Prefix = $" {MessageFormatter.FormatColor(Config.Settings.Prefix)}";

            GroupManager = new Managers.GroupManager(Config.VIPGroups);
            RandomVipManager = new Managers.RandomVipManager(Config.RandomVip, Prefix);
            NightVipManager = new Managers.NightVipManager(Config.NightVip);

            foreach (var _ in Config.VIPGroups)
            {
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
                    if (!_playerCache.ContainsKey(player)) continue;

                    OnTick(player);
                }
            });

            RegisterListener<OnEntitySpawned>(OnEntitySpawned);

            if (hotReload)
            {
                foreach (var player in Utilities.GetPlayers()
                    .Where(p => Managers.PlayerManager.IsValid(p) && !p.IsHLTV))
                {

                    _playerCache.Add(player, new PlayerData { GroupId = GroupManager!.GetPlayerGroup(player) });
                }
            }
        }

        public override void Unload(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
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
                        continue;
                    var playerData = _playerCache[player];
                    if (playerData.GroupId == -1 || Config.VIPGroups[playerData.GroupId] != group)
                        continue;

                    PlayerManager.AddArmor(player, group.Misc.ArmorRegen.Amount);
                }
            });
        }
    }
}

