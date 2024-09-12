using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Core;

public partial class Plugin
{

    private Dictionary<CCSPlayerController, List<Action>> ChatCommandActions = new Dictionary<CCSPlayerController, List<Action>>();
    private Stack<Action> navigationStack = new Stack<Action>();


    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<steamid>")]
    public void OnReloadServicesCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {

        string steamId64String = commandInfo.GetArg(1);
        ulong? steamId64 = GetSteamId(steamId64String);
        if (steamId64 == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}");
            return;
        }

        CCSPlayerController? target = Utilities.GetPlayers().Find(u => u.AuthorizedSteamID!.SteamId64.Equals(steamId64));

        if (target == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}");
            return;
        }

        if (!_playerCache.TryGetValue(target, out PlayerData? playerData))
        {
            playerData = new PlayerData();
            _playerCache.Add(target, playerData);
        }
        Task.Run(async () =>
        {
            bool qualifiesForNightVip = Config.NightVip.Enabled &&
                               NightVipManager!.IsNightVipTime() &&
                               NightVipManager.PlayerQualifies(target);

            await playerData.LoadData(target, GroupManager!, DatabaseManager);

            if (qualifiesForNightVip)
            {
                await Server.NextFrameAsync(() =>
                {
                    NightVipManager!.GiveNightVip(target, Localizer);
                });
            }
            Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.services.reloaded"]}"));
        });
    }

    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<service id>")]
    public void OnServiceEnableCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }


        string serviceIdString = commandInfo.GetArg(1);
        if (!int.TryParse(serviceIdString, out int serviceId))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.invalid_id"]}");
            return;
        }

        string PlayerName = "Console";
        ulong PlayerSteamId = 0;
        if (player != null)
        {
            PlayerName = player.PlayerName;
            PlayerSteamId = player.AuthorizedSteamID!.SteamId64;
        }

        Task.Run(async () =>
        {
            try
            {
                int rowsAffected = await DatabaseManager!.SetServiceAvailability(serviceId, ServiceAvailability.Enabled);
                Server.NextWorldUpdate(() =>
                {
                    if (rowsAffected == 0)
                    {
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.not_found", serviceId]}");
                        return;
                    }

                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.enabled_success", serviceId]}");
                    Logger.LogInformation("Player {name}({steamid}) enabled service id={id}", PlayerName, PlayerSteamId, serviceId);

                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));

                Logger.LogError("Error occured while enabling service: {error}", ex.ToString());
            }
        });
    }

    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<service id>")]
    public void OnServiceDisableCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }

        string serviceIdString = commandInfo.GetArg(1);
        if (!int.TryParse(serviceIdString, out int serviceId))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.invalid_id", serviceIdString]}");
            return;
        }
        string PlayerName = "Console";
        ulong PlayerSteamId = 0;
        if (player != null)
        {
            PlayerName = player.PlayerName;
            PlayerSteamId = player.AuthorizedSteamID!.SteamId64;
        }
        Task.Run(async () =>
        {
            try
            {
                int rowsAffected = await DatabaseManager!.SetServiceAvailability(serviceId, ServiceAvailability.Disabled);
                Server.NextWorldUpdate(() =>
                {
                    if (rowsAffected == 0)
                    {
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.not_found", serviceId]}");
                        return;
                    }

                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.disabled_success", serviceId]}");
                    Logger.LogInformation("Player {name}({steamid}) disabled service id={id}", PlayerName, PlayerSteamId, serviceId);

                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));

                Logger.LogError("Error occured while disabling service: {error}", ex.ToString());
            }
        });

    }

    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<service id>")]
    public void OnServiceDeleteCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }

        string serviceIdString = commandInfo.GetArg(1);
        if (!int.TryParse(serviceIdString, out int serviceId))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.invalid_id", serviceIdString]}");
            return;
        }

        string PlayerName = "Console";
        ulong PlayerSteamId = 0;
        if (player != null)
        {
            PlayerName = player.PlayerName;
            PlayerSteamId = player.AuthorizedSteamID!.SteamId64;
        }

        Task.Run(async () =>
        {
            try
            {
                int rowsAffected = await DatabaseManager!.RemoveService(serviceId);
                Server.NextWorldUpdate(() =>
                {
                    if (rowsAffected == 0)
                    {
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.not_found", serviceId]}");
                        return;
                    }
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.deleted_success", serviceId]}");
                    Logger.LogInformation("Player {name}({steamid}) deleted service id={id}", PlayerName, PlayerSteamId, serviceId);
                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));

                Logger.LogError("Error occured while deleting service: {error}", ex.ToString());
            }
        });

    }

    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<service id>")]
    public void OnServiceInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }

        string serviceIdString = commandInfo.GetArg(1);
        if (!int.TryParse(serviceIdString, out int serviceId))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.invalid_id", serviceIdString]}");
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                PlayerServiceData? service = await DatabaseManager!.GetService(serviceId);
                Server.NextWorldUpdate(() =>
                {
                    if (service == null)
                    {
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.not_found", serviceIdString]}");
                        return;
                    }

                    string flagString = string.Join(", ", service.Flags);

                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.title", 1]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.availability", GetServiceAvailabilityName(service.Availability)]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.flags", flagString]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");

                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));
                Logger.LogError("Error occured while retrieving service: {error}", ex.ToString());
            }
        });

    }


    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, usage: "<steamid64>")]
    public void OnPlayerInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }

        string steamId64String = commandInfo.GetArg(1);
        ulong? steamId64 = GetSteamId(steamId64String);
        if (steamId64 == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}");
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                int? playerId = await DatabaseManager!.GetPlayerIdRaw((ulong)steamId64);
                if (playerId == null)
                {
                    Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["command.invalid_syntax"]}"));

                    return;
                }

                List<PlayerServiceData> serviceData = await DatabaseManager!.GetPlayerServices((int)playerId, ServiceAvailability.Enabled);
                Server.NextWorldUpdate(() =>
                {
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.info.id", playerId]}");
                    commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.info.service_count", serviceData.Count]}");

                    int counter = 1;
                    foreach (var service in serviceData)
                    {
                        string flagString = string.Join(", ", service.Flags);

                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.title", counter++]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.availability", GetServiceAvailabilityName(service.Availability)]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.flags", flagString]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
                        commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
                    }
                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));

                Logger.LogError("Error occured while performing player info: {error}", ex.ToString());
            }
        });

    }



    public void OnPlayerVIPInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo) && player == null)
        {
            return;
        }

        ulong? steamId64 = player!.SteamID;

        Task.Run(async () =>
        {
            try
            {
                int? playerId = await DatabaseManager!.GetPlayerIdRaw((ulong)steamId64);
                if (playerId == null)
                {
                    Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["command.invalid_syntax"]}"));
                    return;
                }

                // Get the player's services
                List<PlayerServiceData> serviceData = await DatabaseManager!.GetPlayerServices((int)playerId, ServiceAvailability.Enabled);

                // Check if player qualifies for NightVIP
                bool isNightVIP = NightVipManager.PlayerQualifies(player);

                

                Server.NextWorldUpdate(() =>
                {
                    player.PrintToChat($"{PluginPrefix}{Localizer["player.info.id", playerId]}");

                    if (isNightVIP)
                    {
                        player.PrintToChat($"{PluginPrefix}{Localizer["player.info.nightvip"]}");
                    }

               
                    if (serviceData.Any())
                    {
                        player.PrintToChat($"{PluginPrefix}{Localizer["player.info.service_count", serviceData.Count]}");

                        var servicesWithGroups = serviceData.Where(s => GroupManager.GetGroup(s.GroupId) != null).ToList();
                        var servicesWithoutGroups = serviceData.Where(s => GroupManager.GetGroup(s.GroupId) == null).ToList();

                        //handle multiple groups menu.
                        if (servicesWithGroups.Count > 1)
                        {
                            CenterHtmlMenu groupMenu = new CenterHtmlMenu(Localizer["Group Selection"], this);

                            foreach (var service in servicesWithGroups)
                            {
                                VipGroupConfig groupConfig = GroupManager.GetGroup(service.GroupId);
                                if (groupConfig != null)
                                {
                                    groupMenu.AddMenuOption($"{groupConfig.Name}", (controller, option) =>
                                    {
                                        DisplayVIPGroupInfo(player, groupConfig);

                                        string flagString = string.Join(", ", service.Flags);
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.title", service.GroupId]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.availability", GetServiceAvailabilityName(service.Availability)]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.flags", flagString]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
                                        player.PrintToChat($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
                                    });
                                }
                            }

                            MenuManager.OpenCenterHtmlMenu(this, player, groupMenu);
                        }
                        else if (servicesWithGroups.Count == 1)
                        {
                            var service = servicesWithGroups.First();
                            VipGroupConfig groupConfig = GroupManager.GetGroup(service.GroupId);
                            if (groupConfig != null)
                            {
                                DisplayVIPGroupInfo(player, groupConfig);

                                string flagString = string.Join(", ", service.Flags);
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.title", service.GroupId]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.availability", GetServiceAvailabilityName(service.Availability)]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.flags", flagString]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
                                player.PrintToChat($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
                            }
                        }
                        foreach (var service in servicesWithoutGroups)
                        {
                            string flagString = string.Join(", ", service.Flags);
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.title", service.Id]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.availability", GetServiceAvailabilityName(service.Availability)]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.flags", flagString]}");
                            player.PrintToChat($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
                        }
                    }
                    else
                    {
                        player.PrintToChat($"{PluginPrefix}{Localizer["player.info.no_service"]}");
                    }

                });
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => player.PrintToCenter($"{PluginPrefix}{Localizer["database.invalid_query"]}"));
                Logger.LogError("Error occurred while performing player info: {error}", ex.ToString());
            }
        });
    }


    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 3, usage: "<steamid64> <duration> <flag1> ...")]
    public void OnPlayerAddFlagsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }

        string steamId64String = commandInfo.GetArg(1);
        string durationString = commandInfo.GetArg(2);

        List<string> flags = [];
        for (int i = 3; i < commandInfo.ArgCount; i++)
        {
            flags.Add(commandInfo.GetArg(i));
        }

        if (!ulong.TryParse(durationString, out ulong duration))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["command.invalid_syntax"]}");
            return;
        }

        ulong? steamId64 = GetSteamId(steamId64String);
        if (steamId64 == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}");
            return;
        }

        ulong issuerSteamid64 = 0;
        if (player != null && player.AuthorizedSteamID != null)
        {
            issuerSteamid64 = player.AuthorizedSteamID.SteamId64;
        }

        DateTime endTime = DateTime.UtcNow.AddMinutes(duration);

        Task.Run(async () =>
        {
            try
            {
                int? targetId = await DatabaseManager!.GetPlayerIdRaw((ulong)steamId64);
                if (targetId == null)
                {
                    Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}"));
                    return;
                }

                string flagString = string.Join(", ", flags);
                int serviceId = await DatabaseManager!.AddNewService((int)targetId, DateTime.UtcNow, endTime, flagString, "", $"cmd: {issuerSteamid64}");

                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.add.flags.success",
                    flagString, steamId64, targetId, duration
                ]}"));

            }
            catch (Exception ex)
            {
                commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}");
                Logger.LogError("Encountered error while adding new service: {error}", ex.ToString());
            }
        });

    }

    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 3, usage: "<steamid64> <duration in minutes> <group id>")]
    public void OnPlayerAddGroupCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!HandleDatabaseCommand(player, commandInfo))
        {
            return;
        }


        string durationString = commandInfo.GetArg(2);
        string groupId = commandInfo.GetArg(3);

        if (!ulong.TryParse(durationString, out ulong duration))
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["command.invalid_syntax"]}");
            return;
        }

        string steamId64String = commandInfo.GetArg(1);
        ulong? steamId64 = GetSteamId(steamId64String);
        if (steamId64 == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}");
            return;
        }

        ulong issuerSteamid64 = 0;
        if (player != null && player!.AuthorizedSteamID != null)
        {
            issuerSteamid64 = player.AuthorizedSteamID.SteamId64;
        }

        VipGroupConfig? group = GroupManager!.GetGroup(groupId);
        if (group == null)
        {
            commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["group.not_found", groupId]}");
            return;
        }

        DateTime endTime = DateTime.UtcNow.AddMinutes(duration);

        Task.Run(async () =>
        {
            try
            {
                int? targetId = await DatabaseManager!.GetPlayerIdRaw((ulong)steamId64);
                if (targetId == null)
                {
                    Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.id.not_found"]}"));
                    return;
                }

                int serviceId = await DatabaseManager!.AddNewService((int)targetId, DateTime.UtcNow, endTime, "", groupId, $"cmd: {issuerSteamid64}");

                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["player.add.group.success", group.Name, steamId64, targetId, duration]}"));
            }
            catch (Exception ex)
            {
                Server.NextWorldUpdate(() => commandInfo.ReplyToCommand($"{PluginPrefix}{Localizer["database.invalid_query"]}"));

                Logger.LogError("Encountered error while adding new service: {error}", ex.ToString());
            }
        });

    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_vipdebug", "Vip plugin debug command")]
    public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null)
        {
            return;
        }

        player.PrintToChat($"{PluginPrefix} [VIP-Plugin] Debug info has been printed in the console.");

        player.PrintToConsole("---Group data ---");
        if (!_playerCache.TryGetValue(player, out PlayerData? playerData))
        {
            player.PrintToConsole("* Your group is null");
        }
        else
        {
            var pGroup = playerData.Group!;
            player.PrintToConsole($"{PluginPrefix}Name: {pGroup.Name}");
            player.PrintToConsole($"{PluginPrefix}Priority: {pGroup.Priority}");
            player.PrintToConsole($"{PluginPrefix}UniqueId: {pGroup.UniqueId}");
            player.PrintToConsole($"{PluginPrefix}Permissions: {pGroup.Permissions}");
            player.PrintToConsole($"{PluginPrefix}---All group perms---");

            foreach (var group in Config!.VIPGroups)
            {
                var hasPerms = AdminManager.PlayerHasPermissions(player, group.Permissions);
                player.PrintToConsole($"{PluginPrefix}{group.Name} | {group.Permissions}: {(hasPerms ? "yes" : "no")}");
            }
        }

        player.PrintToConsole($"{PluginPrefix}---RandomVIP---");

        player.PrintToConsole($"{PluginPrefix}Enabled: {Config.RandomVip.Enabled}");
        player.PrintToConsole($"{PluginPrefix}AfterRound: {Config.RandomVip.AfterRound}");
        player.PrintToConsole($"{PluginPrefix}Minimum players: {Config.RandomVip.MinimumPlayers}");
        player.PrintToConsole($"{PluginPrefix}RepeatPicking: {Config.RandomVip.RepeatPickingMessage}");
        player.PrintToConsole($"{PluginPrefix}PermissionsGranted: {string.Join(", ", Config.RandomVip.PermissionsGranted)}");
        player.PrintToConsole($"{PluginPrefix}PermissionExclude: {string.Join(", ", Config.RandomVip.PermissionsExclude)}");

        player.PrintToConsole($"{PluginPrefix}---NightVIP---");

        player.PrintToConsole($"{PluginPrefix}Enabled: {Config.NightVip.Enabled}");
        player.PrintToConsole($"{PluginPrefix}is time: {NightVipManager!.IsNightVipTime()}");
        player.PrintToConsole($"{PluginPrefix}StartHour: {Config.NightVip.StartHour}");
        player.PrintToConsole($"{PluginPrefix}EndHour: {Config.NightVip.EndHour}");
        player.PrintToConsole($"{PluginPrefix}RequiredNickPhrase: {Config.NightVip.RequiredNickPhrase}");
        player.PrintToConsole($"{PluginPrefix}RequiredScoreboardTag: {Config.NightVip.RequiredScoreboardTag}");
        player.PrintToConsole($"{PluginPrefix}PermissionsGranted: {string.Join(", ", Config.NightVip.PermissionsGranted)}");
        player.PrintToConsole($"{PluginPrefix}PermissionExclude: {string.Join(", ", Config.NightVip.PermissionsExclude)}");
    }

    private bool HandleDatabaseCommand(CCSPlayerController? player, CommandInfo? command)
    {

        if (!Config.Settings.Database.Enabled ||
            DatabaseManager == null ||
            !DatabaseManager.Initialized)
        {
            command!.ReplyToCommand($"{PluginPrefix}{Localizer["database.not_ready"]}");

            if (player == null)
            {
                Logger.LogError("Couldn't execute a command for {steamId64} because database is not ready.", "CONSOLE");
            }
            else
            {
                ulong? steamId64 = player!.AuthorizedSteamID?.SteamId64;
                Logger.LogError("Couldn't execute a command for {steamId64} because database is not ready.", steamId64);
            }

            return false;
        }

        return true;
    }

    private ulong? GetSteamId(string steamId64String)
    {
        if (ulong.TryParse(steamId64String, out ulong steamId64))
        {
            return steamId64;
        }

        var player = Utilities.GetPlayers().Find(u =>
            u.AuthorizedSteamID != null && !u.IsBot && u.PlayerName.Equals(steamId64String, StringComparison.OrdinalIgnoreCase));
        if (player != null)
        {
            return player.AuthorizedSteamID?.SteamId64;
        }

        return null;
    }


    // Main function to display VIP group properties and handle their selection
    public void DisplayVIPGroupInfo(CCSPlayerController player, VipGroupConfig vipGroup)
    {
        navigationStack.Clear();

        CenterHtmlMenu menu = new CenterHtmlMenu(Localizer["VIP Group Info"], this);

        Type vipGroupType = vipGroup.GetType();
        var properties = vipGroupType.GetProperties()
            .Where(p => !ShouldExcludeProperty(p.Name))
            .ToList();

        int optionNumber = 1;
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(vipGroup);

            string displayValue = property.Name.Length <= 26 ? property.Name : property.Name.Substring(0, 26);

            if (propertyValue != null && !IsPrimitiveOrString(propertyValue) && !IsCollection(propertyValue))
            {
                menu.AddMenuOption($"{displayValue}", (controller, option) =>
                {
                    navigationStack.Push(() => DisplayVIPGroupInfo(player, vipGroup));

                    DisplaySubMenu(player, property.Name, propertyValue, vipGroup);
                });
            }
            else
            {
                string formattedValue = FormatValue(propertyValue, property.Name);
                menu.AddMenuOption($"{formattedValue}", (controller, option) => { });
            }

            optionNumber++;
        }

        MenuManager.OpenCenterHtmlMenu(this, player, menu);
    }

    private void DisplaySubMenu(CCSPlayerController player, string menuTitle, object nestedObject, VipGroupConfig vipGroup)
    {

        CenterHtmlMenu submenu = new CenterHtmlMenu(Localizer[menuTitle.ToLower()], this);

        submenu.AddMenuOption(Localizer["Go Back"], (controller, option) =>
        {
            if (navigationStack.Count > 0)
            {
                var previousMenu = navigationStack.Pop();
                previousMenu();
            }
        });

        Type nestedType = nestedObject.GetType();
        var properties = nestedType.GetProperties()
            .Where(p => !ShouldExcludeProperty(p.Name)) 
            .ToList();

        int lineNumber = 2;
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(nestedObject);
            string localizedPropertyName = Localizer[$"{menuTitle.ToLower()}.{property.Name.ToLower()}"];

            if (propertyValue != null && !IsPrimitiveOrString(propertyValue) && !IsCollection(propertyValue))
            {
                // If the property is an object, create a submenu for it
                submenu.AddMenuOption($"{localizedPropertyName}", (controller, option) =>
                {
                    navigationStack.Push(() => DisplaySubMenu(player, menuTitle, nestedObject, vipGroup));
                    DisplaySubMenu(player, property.Name, propertyValue, vipGroup);
                });
            }
            else
            {
                // Display simple properties (e.g., bool, int, etc.) directly with color formatting
                string displayValue = FormatValue(propertyValue, localizedPropertyName);
                submenu.AddMenuOption($"{displayValue}", (controller, option) => { });
            }

            lineNumber++;
        }

        MenuManager.OpenCenterHtmlMenu(this, player, submenu);
    }


    private string FormatValue(object value, string propertyName)
    {
        string formattedValue = GetDisplayValue(value);
        return $"{propertyName}: {formattedValue}";
    }

    private string GetDisplayValue(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Yes" : "No";
        }
        if (IsCollection(value))
        {
            var collectionValues = string.Join(", ", ((IEnumerable)value).Cast<object>());
            return collectionValues;
        }
        return value?.ToString() ?? "N/A";
    }

    private bool ShouldExcludeProperty(string propertyName)
    {
        string[] excludedProperties = { "Permissions", "Priority", "UniqueId", "Messages" };
        return excludedProperties.Contains(propertyName);
    }

    private bool IsCollection(object obj)
    {
        return obj is IEnumerable && !(obj is string);
    }

    private bool IsPrimitiveOrString(object value)
    {
        return value == null || value.GetType().IsPrimitive || value is string;
    }
}
