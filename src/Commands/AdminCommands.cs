using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace Core;

public partial class Plugin
{
	[CommandHelper(minArgs: 1, usage: "<service id>")]
	public void OnServiceEnableCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string serviceIdString = commandInfo.GetArg(1);
			if (!int.TryParse(serviceIdString, out int serviceId))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["service.invalid_id"]}");
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					int rowsAffected = await DatabaseManager!.SetServiceAvailability(serviceId, Models.ServiceAvailability.Enabled);
					await Server.NextFrameAsync(() =>
					{
						if (rowsAffected == 0)
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.not_found"]}");
						}
						else
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.enabled_success"]}");
							Logger.LogInformation("Player {name}({steamid}) enabled service id={id}", player.PlayerName, player.AuthorizedSteamID!.SteamId64, serviceId);
						}
					});
				}
				catch (Exception ex)
				{
					player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					Logger.LogError("Error occured while enabling service: {error}", ex.ToString());
				}
			});
		});
	}

	[CommandHelper(minArgs: 1, usage: "<service id>")]
	public void OnServiceDisableCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string serviceIdString = commandInfo.GetArg(1);
			if (!int.TryParse(serviceIdString, out int serviceId))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["service.invalid_id"]}");
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					int rowsAffected = await DatabaseManager!.SetServiceAvailability(serviceId, Models.ServiceAvailability.Disabled);
					await Server.NextFrameAsync(() =>
					{
						if (rowsAffected == 0)
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.not_found"]}");
						}
						else
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.disabled_success"]}");
							Logger.LogInformation("Player {name}({steamid}) disabled service id={id}", player.PlayerName, player.AuthorizedSteamID!.SteamId64, serviceId);
						}
					});
				}
				catch (Exception ex)
				{
					player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					Logger.LogError("Error occured while disabling service: {error}", ex.ToString());
				}
			});
		});
	}

	[CommandHelper(minArgs: 1, usage: "<service id>")]
	public void OnServiceDeleteCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string serviceIdString = commandInfo.GetArg(1);
			if (!int.TryParse(serviceIdString, out int serviceId))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["service.invalid_id"]}");
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					int rowsAffected = await DatabaseManager!.RemoveService(serviceId);
					await Server.NextFrameAsync(() =>
					{
						if (rowsAffected == 0)
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.not_found"]}");
						}
						else
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.deleted_success"]}");
							Logger.LogInformation("Player {name}({steamid}) deleted service id={id}", player.PlayerName, player.AuthorizedSteamID!.SteamId64, serviceId);
						}
					});
				}
				catch (Exception ex)
				{
					player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					Logger.LogError("Error occured while deleting service: {error}", ex.ToString());
				}
			});
		});
	}

	[CommandHelper(minArgs: 1, usage: "<service id>")]
	public void OnServiceInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string serviceIdString = commandInfo.GetArg(1);
			if (!int.TryParse(serviceIdString, out int serviceId))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["service.invalid_id"]}");
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					PlayerServiceData? service = await DatabaseManager!.GetService(serviceId);
					await Server.NextFrameAsync(() =>
					{
						if (service == null)
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.not_found"]}");
						}
						else
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.player_id", service.PlayerId]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.availability", service.Availability]}"); // TO:DO format name
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.flags", service.Flags]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
						}
					});
				}
				catch (Exception ex)
				{
					Server.NextFrame(() =>
					{
						player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					});
					Logger.LogError("Error occured while retrieving service: {error}", ex.ToString());
				}
			});
		});
	}

	[CommandHelper(minArgs: 1, usage: "<steamid64>")]
	public void OnPlayerInfoCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string steamId64String = commandInfo.GetArg(1);
			if (!ulong.TryParse(steamId64String, out ulong steamId64))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["player.info.invalid_command"]}");
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					int playerId = await DatabaseManager!.GetPlayerIdRaw(steamId64);
					List<PlayerServiceData> serviceData = await DatabaseManager!.GetPlayerServices(playerId, ServiceAvailability.Enabled);
					await Server.NextFrameAsync(() =>
					{
						player!.PrintToChat($"{PluginPrefix}{Localizer["player.info.id", playerId]}");
						player!.PrintToChat($"{PluginPrefix}{Localizer["player.info.service_count", serviceData.Count]}");
						foreach(var service in serviceData) 
						{
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.id", service.Id]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.availability", service.Availability]}"); // TO:DO format name
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.start", service.Start]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.end", service.End]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.flags", service.Flags]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.group_id", service.GroupId]}");
							player!.PrintToChat($"{PluginPrefix}{Localizer["service.info.notes", service.Notes]}");
						}
					});
				}
				catch (Exception ex)
				{
					player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					Logger.LogError("Error occured while performing player info: {error}", ex.ToString());
				}
			});
		});
		return;
	}

	[CommandHelper(minArgs: 3, usage: "<steamid64> <duration> <flag1> ...")]
	public void OnPlayerAddFlagsCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string steamid64String = commandInfo.GetArg(1);
			string durationString = commandInfo.GetArg(2);

			List<string> flags = [];
			for (int i = 3; i < commandInfo.ArgCount; i++)
			{
				flags.Add(commandInfo.GetArg(i));
			}

			if (!ulong.TryParse(steamid64String, out ulong steamid64) ||
				!ulong.TryParse(durationString, out ulong duration))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["player.add.invalid_command"]}"); // to:do better name
				return;
			}

			ulong issuerSteamid64 = 0;
			if (player!.AuthorizedSteamID != null)
			{
				issuerSteamid64 = player.AuthorizedSteamID.SteamId64;
			}

			DateTime endTime = DateTime.UtcNow.AddMinutes(duration);

			Task.Run(async () =>
			{
				try
				{
					int? targetId = await DatabaseManager!.GetPlayerIdRaw(steamid64);
					if (targetId == null)
					{
						player!.PrintToChat($"{PluginPrefix}{Localizer["database.player_not_found"]}");
						return;
					}

					string flagString = string.Join(", ", flags);
					int serviceId = await DatabaseManager!.AddNewService((int)targetId, DateTime.UtcNow, endTime, flagString, "", $"cmd: {issuerSteamid64}");
				}
				catch (Exception ex)
				{
					Logger.LogError("Encountered error while adding new service: {error}", ex.ToString());
					Server.NextFrame(() =>
					{
						player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					});
					return;
				}
			});
		});
		return;
	}

	[CommandHelper(minArgs: 3, usage: "<steamid64> <duration in minutes> <group id>")]
	public void OnPlayerAddGroupCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		HandleDatabaseCommand(player, commandInfo, () =>
		{
			string steamid64String = commandInfo.GetArg(1);
			string durationString = commandInfo.GetArg(2);
			string groupId = commandInfo.GetArg(3);

			if (!ulong.TryParse(steamid64String, out ulong steamid64) ||
				!ulong.TryParse(durationString, out ulong duration))
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["player.add.invalid_command"]}"); // to:do better name
				return;
			}

			ulong issuerSteamid64 = 0;
			if (player!.AuthorizedSteamID != null)
			{
				issuerSteamid64 = player.AuthorizedSteamID.SteamId64;
			}

			VipGroupConfig? group = GroupManager!.GetGroup(groupId);
			if (group == null)
			{
				player!.PrintToChat($"{PluginPrefix}{Localizer["service.not_found"]}"); // to:do better name
				return;
			}

			DateTime endTime = DateTime.UtcNow.AddMinutes(duration);

			Task.Run(async () =>
			{
				try
				{
					int? targetId = await DatabaseManager!.GetPlayerIdRaw(steamid64);
					if (targetId == null)
					{
						player!.PrintToChat($"{PluginPrefix}{Localizer["database.player_not_found"]}");
						return;
					}

					int serviceId = await DatabaseManager!.AddNewService((int)targetId, DateTime.UtcNow, endTime, "", groupId, $"cmd: {issuerSteamid64}");
				}
				catch (Exception ex)
				{
					Logger.LogError("Encountered error while adding new service: {error}", ex.ToString());
					Server.NextFrame(() =>
					{
						player?.PrintToChat($"{PluginPrefix}{Localizer["database.invalid_query"]}");
					});
					return;
				}
			});
		});

		return;
	}
	public void OnServicesCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{

		return;
	}

	[RequiresPermissions("@css/root")]
	[ConsoleCommand("css_vipdebug", "Vip plugin debug command")]
	public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
	{
		if (player == null)
		{
			return;
		}

		player.PrintToChat($"{PluginPrefix}{ChatColors.Red}[VIP-Plugin] Debug info has been printed in the console.");

		player.PrintToConsole("---Group data ---");
		if (!_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
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
				player.PrintToChat($"{PluginPrefix}{group.Name}-{group.Permissions}-{(hasPerms ? "yes" : "no")}");
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

	private void HandleDatabaseCommand(CCSPlayerController? player, CommandInfo commandInfo, Action action)
	{
		if (player == null)
		{
			return;
		}

		if (DatabaseManager == null ||
			!DatabaseManager.Initialized)
		{
			player.PrintToChat($"{PluginPrefix}{Localizer["database.not_ready"]}");
			return;
		}

		action.Invoke();
	}
}
