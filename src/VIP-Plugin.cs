using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Core;

public partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.1.0";
    public required PluginConfig Config { get; set; }

    private GroupManager? GroupManager { get; set; }
    private RandomVipManager? RandomVipManager { get; set; }
    private NightVipManager? NightVipManager { get; set; }

    private DatabaseManager? DatabaseManager { get; set; }

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

        if (Config.Settings.DatabaseVips.Enabled)
        {
            DatabaseManager = new DatabaseManager(Config.Settings.DatabaseVips.SqlConfig);
            Task.Run(async () =>
            {
                try
                {
                    await DatabaseManager.Initialize();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error while initializing database: {message}", ex.ToString());
                }
            });
        }

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
                var playerData = new PlayerData();
                _playerCache.Add(player, playerData);

                Task.Run(async () =>
                {
                    await playerData.LoadData(player, GroupManager!, DatabaseManager!);

                    await Server.NextFrameAsync(() =>
                    {
                        PermissionManager.AddPermissions(player, playerData.DatabaseData.AllFlags);
                    });
                });
            }
        }

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

    public void RegisterCommands()
    {
        var cmdConfig = Config.Settings.DatabaseVips.Commands;
        RegisterCommandIfEnabled("css_vp_service_enable", "Enable service", OnServiceEnableCommand, cmdConfig.ServiceEnable);
        RegisterCommandIfEnabled("css_vp_service_disable", "Disable service", OnServiceDisableCommand, cmdConfig.ServiceDisable);
        RegisterCommandIfEnabled("css_vp_service_delete", "Delete service", OnServiceDeleteCommand, cmdConfig.ServiceDelete);
        RegisterCommandIfEnabled("css_vp_service_info", "Service information", OnServiceInfoCommand, cmdConfig.ServiceInfo);
        RegisterCommandIfEnabled("css_vp_player_info", "Player information", OnPlayerInfoCommand, cmdConfig.PlayerInfo);
        RegisterCommandIfEnabled("css_vp_player_removeall", "Remove all players", OnPlayerRemoveAllCommand, cmdConfig.PlayerRemoveAll);
        RegisterCommandIfEnabled("css_vp_player_addflags", "Add flags to player", OnPlayerAddFlagsCommand, cmdConfig.PlayerAddFlags);
        RegisterCommandIfEnabled("css_vp_player_addgroup", "Add group to player", OnPlayerAddGroupCommand, cmdConfig.PlayerAddGroup);
        RegisterCommandIfEnabled("css_services", "List available services", OnServicesCommand, cmdConfig.Services);
    }

    private void RegisterCommandIfEnabled(string commandName, string description, CommandInfo.CommandCallback callback, CommandConfig config)
    {
        if (config.Enabled)
        {
            RegisterCommandWithAlias(commandName, description, callback, config.Alias);
        }
    }

    public void RegisterCommandWithAlias(string commandName, string description, CommandInfo.CommandCallback callback, List<String> alias)
    {
        AddCommand(commandName, description, callback);
        foreach (var aliasName in alias)
        {
            AddCommand(aliasName, description, callback);
        }
    }
}

