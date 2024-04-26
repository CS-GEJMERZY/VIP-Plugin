using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Core
{
    public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "VIP Plugin";
        public override string ModuleAuthor => "Hacker";
        public override string ModuleVersion => "1.0.27";
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

            // TO:DO - actually fix it(look #30)
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        }
        public override void Unload(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        }

        private static CCSGameRules GetGamerules()
        {
            return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        }
    }
}

