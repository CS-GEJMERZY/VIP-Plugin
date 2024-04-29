﻿using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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

        if (Config.Settings.EnableDatabaseVips)
        {
            DatabaseManager = new DatabaseManager(Config.Settings.Database);
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
}

