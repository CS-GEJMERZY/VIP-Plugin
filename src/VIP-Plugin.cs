using Core.Config;
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
        public override string ModuleVersion => "1.0.20";

        public required PluginConfig Config { get; set; }

        private Managers.GroupManager? GroupManager { get; set; }
        private Managers.RandomVipManager? RandomVipManager { get; set; }
        private Managers.NightVipManager? NightVipManager { get; set; }

        private readonly Dictionary<CCSPlayerController, PlayerData> _playerCache = [];

        public void OnConfigParsed(PluginConfig _Config)
        {
            Config = _Config;
            string Prefix = $" {MessageFormatter.FormatColor(Config.Settings.Prefix)}";

            GroupManager = new Managers.GroupManager(Config.VIPGroups);
            RandomVipManager = new Managers.RandomVipManager(Config.RandomVip, Prefix);
            NightVipManager = new Managers.NightVipManager(Config.NightVip);
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
    }
}

