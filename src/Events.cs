using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace VIP;

public partial class VIPlugin
{
	[GameEventHandler]
	public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
	{
		CCSPlayerController player = @event.Userid;

		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
		{
			return HookResult.Continue;
		}


		AddTimer(1.0f, () =>
		{
			PlayerCache.Add(player, new VIPPlayer());
			PlayerCache[player].LoadGroup(player, GroupManager!);

			int GroupID = PlayerCache[player].GroupID;
			if (GroupID != -1)
			{
				if (Config.Groups[GroupID].ConnectMessage != string.Empty)
				{
					string message = Config.Groups[GroupID].ConnectMessage;
					message = message.Replace("{playername}", player.PlayerName);

					Server.PrintToChatAll(PluginMessageFormatter.FormatColor(message));
				}
			}

		});

		return HookResult.Continue;
	}

	private void OnClientDisconnect(int playerSlot)
	{
		CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.IsHLTV)
		{
			return;
		}

		int GroupID = PlayerCache[player].GroupID;
		if (GroupID != -1)
		{
			if (Config.Groups[GroupID].DisconnectMessage != string.Empty)
			{
				string message = Config.Groups[GroupID].DisconnectMessage;
				message = message.Replace("{playername}", player.PlayerName);

				Server.PrintToChatAll(PluginMessageFormatter.FormatColor(message));
			}
		}

		if(PlayerCache.ContainsKey(player))
		{
            PlayerCache.Remove(player);
        }
	}


	[GameEventHandler]
	public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		CCSPlayerController player = @event.Userid;
		if (player == null || !player.IsValid || !player.PlayerPawn.IsValid || player.IsBot)
		{
			return HookResult.Continue;
		}

		// They key might not be present before the player is fully authorized in EventPlayerConnectFull
		if (!PlayerCache.ContainsKey(player))
		{
			return HookResult.Continue;
		}

		PlayerCache[player].LoadGroup(player, GroupManager!);
		int playerGroupID = PlayerCache[player].GroupID;
		if (playerGroupID == -1)
		{
			return HookResult.Continue;
		}
		AddTimer(1.0f, () =>
		{
			PlayerSpawnn_TimerGive(player, playerGroupID);
		});

		return HookResult.Continue;
	}

	private void PlayerSpawnn_TimerGive(CCSPlayerController player, int playerGroupID)
	{
		if (!player.IsValid || !player.PawnIsAlive || player.PlayerPawn == null) { return; }

		VIPGroup playerGroup = Config.Groups[playerGroupID];

		player.PlayerPawn!.Value!.Health = playerGroup.SpawnHP; // HP
		player.InGameMoneyServices!.Account += playerGroup.SpawnMoney; // Money

		player.PlayerPawn.Value.ArmorValue = playerGroup.SpawnArmor; // Armor

		if (playerGroup.SpawnHelmet) { player.GiveNamedItem(CsItem.KevlarHelmet); }


		if (playerGroup.SpawnDefuser && player.TeamNum is (int)CsTeam.CounterTerrorist && !player.PawnHasDefuser)
		{
			player.GiveNamedItem("item_defuser"); // Defuser
		}

		// grenades

		for (int i = 0; i < playerGroup.SmokeGrenade; i++)
		{
			player.GiveNamedItem(CsItem.SmokeGrenade);
		}



		for (int i = 0; i < playerGroup.HEGrenade; i++)
		{
			player.GiveNamedItem(CsItem.HEGrenade);
		}



		for (int i = 0; i < playerGroup.FlashGrenade; i++)
		{
			player.GiveNamedItem(CsItem.FlashbangGrenade);
		}



		for (int i = 0; i < playerGroup.Molotov; i++)
		{
			switch (player.TeamNum)
			{
				case (int)CsTeam.Terrorist:
					player.GiveNamedItem(CsItem.Molotov);
					break;

				case (int)CsTeam.CounterTerrorist:
					player.GiveNamedItem(CsItem.IncendiaryGrenade);
					break;
			}

		}

		for (int i = 0; i < playerGroup.DecoyGrenade; i++)
		{
			player.GiveNamedItem(CsItem.DecoyGrenade);
		}

        bool hasZeus = false;

        foreach (var weapon in player!.PlayerPawn!.Value!.WeaponServices!.MyWeapons)
        {
            if (weapon.IsValid && weapon!.Value!.IsValid)
            {
                if (weapon.Value.DesignerName == "weapon_taser")
                {
                    hasZeus = true;
                    break;
                }
            }
        }

        if (!hasZeus)
        {
            player.GiveNamedItem(CsItem.Zeus);
        }



        AddTimer(0.5f,() => { 
			VIPPlayer.RefreshUI(player, player.PlayerPawn!.Value!.WeaponServices!.ActiveWeapon!.Value!.As<CCSWeaponBase>().VData!.GearSlot); 
		});
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
	{
		foreach (CCSPlayerController player in Utilities.GetPlayers())
		{
			if (player == null || !player.IsValid || !player.PlayerPawn.IsValid || player.IsBot)
			{
				continue;
			}

			if (!PlayerCache.ContainsKey(player))
			{
				return HookResult.Continue;
			}

			int playerGroupID = PlayerCache[player].GroupID;
			if (playerGroupID != -1)
			{
				VIPGroup playerGroup = Config.Groups[playerGroupID];

				player.InGameMoneyServices!.Account += playerGroup.RoundWonMoney;
			}
		}

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
	{
		CCSPlayerController attacker = @event.Attacker;

		if (attacker == null || !attacker.IsValid || attacker.IsBot)
		{
			return HookResult.Continue;
		}

		if (!PlayerCache.ContainsKey(attacker))
		{
			return HookResult.Continue;
		}

		int playerGroupID = PlayerCache[attacker].GroupID;
		if (playerGroupID != -1)
		{
			VIPGroup playerGroup = Config.Groups[playerGroupID];

			if (@event.Headshot)
			{
				attacker.InGameMoneyServices!.Account += playerGroup.HeadshotKillMoney;
				attacker.PlayerPawn!.Value!.Health += playerGroup.HeadshotKillHP;

				//VIPPlayer.RefreshUI(attacker);
			}
			else
			{
				attacker.InGameMoneyServices!.Account += playerGroup.KillMoney;
				attacker.PlayerPawn!.Value!.Health += playerGroup.KillHP;

				//VIPPlayer.RefreshUI(attacker);
			}
		}

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
	{
		CCSPlayerController player = @event.Userid;

		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
		{
			return HookResult.Continue;
		}

		if (!PlayerCache.ContainsKey(player))
		{
			return HookResult.Continue;
		}

		int playerGroupID = PlayerCache[player].GroupID;
		if (playerGroupID != -1)
		{
			VIPGroup playerGroup = Config.Groups[playerGroupID];
			player.InGameMoneyServices!.Account += playerGroup.BombPlantMoney;

			//VIPPlayer.RefreshUI(player);
		}

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
	{
		CCSPlayerController player = @event.Userid;

		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
		{
			return HookResult.Continue;
		}

		if (!PlayerCache.ContainsKey(player))
		{
			return HookResult.Continue;
		}

		int playerGroupID = PlayerCache[player].GroupID;
		if (playerGroupID != -1)
		{
			VIPGroup playerGroup = Config.Groups[playerGroupID];
			player.InGameMoneyServices!.Account += playerGroup.BombDefuseMoney;


		}

		return HookResult.Continue;
	}


	[GameEventHandler(HookMode.Post)]
	public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
	{
		CCSPlayerController player = @event.Userid;
		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
		{
			return HookResult.Continue;
		}

		if (!PlayerCache.ContainsKey(player)) { return HookResult.Continue; }

		int playerGroupID = PlayerCache[player].GroupID;
		if (playerGroupID != -1)
		{
			VIPGroup playerGroup = Config.Groups[playerGroupID];
			if (@event.Hitgroup == 0 &&
				!new[] { "inferno", "hegrenade", "knife" }.Contains(@event.Weapon) &&
				playerGroup.NoFallDamage)
			{
				player.PlayerPawn!.Value!.Health += @event.DmgHealth;
			}
		}

		return HookResult.Continue;
	}

	private void OnTick(CCSPlayerController player)
	{
		if (player == null)
		{
			return;
		}

		if (!PlayerCache.ContainsKey(player))
		{
			return;
		}

		VIPPlayer playerData = PlayerCache[player];
		if (playerData.GroupID == -1) { return; }

		VIPGroup playerGroup = Config.Groups[playerData.GroupID];
		if (playerGroup.ExtraJumps == 0)
		{
			return;
		}

		CCSPlayerPawn pawn = player.PlayerPawn!.Value!;

		if (pawn != null)
		{
			var flags = (PlayerFlags)pawn.Flags;
			var buttons = player.Buttons;

			var lastFlags = PlayerCache[player].TempData.LastFlags;
			var lastButtons = PlayerCache[player].TempData.LastButtons;

			if ((lastFlags & PlayerFlags.FL_ONGROUND) != 0 &&
				 (flags & PlayerFlags.FL_ONGROUND) == 0 &&
				 (lastButtons & PlayerButtons.Jump) == 0 &&
				 (buttons & PlayerButtons.Jump) != 0)
			{
				PlayerCache[player].TempData.JumpsUsed++;
			}
			else if ((flags & PlayerFlags.FL_ONGROUND) != 0)
			{
				PlayerCache[player].TempData.JumpsUsed = 0;
			}
			else if ((lastButtons & PlayerButtons.Jump) == 0 &&
				(buttons & PlayerButtons.Jump) != 0 &&
				PlayerCache[player].TempData.JumpsUsed <= playerGroup.ExtraJumps)
			{
				PlayerCache[player].TempData.JumpsUsed++;

				const float Z_VELOCITY = (float)250.0;
				pawn.AbsVelocity.Z = Z_VELOCITY;

			}

			PlayerCache[player].TempData.LastFlags = flags;
			PlayerCache[player].TempData.LastButtons = buttons;
		}
	}
}