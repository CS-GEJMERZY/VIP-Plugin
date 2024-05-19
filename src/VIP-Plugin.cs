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

public sealed partial class Plugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "VIP Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.2.0";
    public override string ModuleDescription => "https://github.com/CS-GEJMERZY/VIP-Plugin";

    public required PluginConfig Config { get; set; }

    public DatabaseManager? DatabaseManager { get; set; }
    public GroupManager? GroupManager { get; set; }
    public RandomVipManager? RandomVipManager { get; set; }
    public NightVipManager? NightVipManager { get; set; }
    public TestVipManager? TestVipManager { get; set; }

    private readonly Dictionary<CCSPlayerController, PlayerData> _playerData = [];

    private readonly List<Timer?> _healthRegenTimers = [];
    private readonly List<Timer?> _armorRegenTimers = [];

    public string PluginPrefix { get; set; } = string.Empty;

    private bool DatabaseVipsEnabled => Config.Settings.Database.Enabled &&
                                       Config.Settings.DatabaseVips.Enabled;

    private bool TestVipEnabled => Config.Settings.Database.Enabled &&
                                   Config.TestVip.Enabled;

    public void OnConfigParsed(PluginConfig _Config)
    {
        Config = _Config;
        PluginPrefix = $" {MessageFormatter.FormatColor(Config.Settings.Prefix)}";

        GroupManager = new GroupManager(Config.VIPGroups);
        RandomVipManager = new RandomVipManager(Config.RandomVip, PluginPrefix);
        NightVipManager = new NightVipManager(Config.NightVip);

        if (Config.Settings.Database.Enabled)
        {
            DatabaseManager = new DatabaseManager(Config.Settings.Database.SqlServer);

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
            _healthRegenTimers.Add(null);
            _armorRegenTimers.Add(null);
        }
    }

    public override void Load(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

        RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        RegisterListener<OnTick>(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                OnTick(player);
            }
        });

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers()
                .Where(p => PlayerManager.IsValid(p) && !p.IsHLTV && !p.IsBot))
            {
                var playerData = new PlayerData();
                _playerData.Add(player, playerData);

                Task.Run(async () =>
                {
                    try
                    {
                        await Server.NextFrameAsync(() =>
                        {
                            playerData.LoadBaseGroup(player, GroupManager!);
                        });

                        if (DatabaseVipsEnabled)
                        {
                            await playerData.LoadDatabaseVipDataAsync(player, GroupManager!, DatabaseManager!);
                        }

                        if (TestVipEnabled)
                        {
                            await playerData.LoadTestVipDataAsync(player, GroupManager!, DatabaseManager!, Config.TestVip);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error while hotreloading player: {error}", ex.ToString());
                    }
                });
            }
        }

        if (DatabaseVipsEnabled)
        {
            RegisterCommands();
        }

        if (TestVipEnabled)
        {
            AddCommand("css_testvip", "Test vipe", OnTestVipCommand);

        }
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
        RegisterCommandIfEnabled("css_vp_player_addflags", "Add flags to player", OnPlayerAddFlagsCommand, cmdConfig.PlayerAddFlags);
        RegisterCommandIfEnabled("css_vp_player_addgroup", "Add group to player", OnPlayerAddGroupCommand, cmdConfig.PlayerAddGroup);
    }

    private void RegisterCommandIfEnabled(string commandName, string description, CommandInfo.CommandCallback callback, CommandConfig config)
    {
        if (config.Enabled)
        {
            AddCommand(commandName, description, callback);
        }
    }
}

