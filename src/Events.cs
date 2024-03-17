using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using Plugin.Managers;

namespace Plugin;

public partial class VipPlugin
{
    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) || player.IsBot || player.IsHLTV)
        {
            return HookResult.Continue;
        }


        AddTimer(1.0f, () =>
        {
            PlayerCache.Add(player, new Models.PlayerData());
            PlayerCache[player].GroupId = groupManager!.GetPlayerGroup(player);

            int GroupID = PlayerCache[player].GroupId;
            if (GroupID != -1)
            {
                if (Config!.Groups[GroupID].ConnectMessage != string.Empty)
                {
                    string message = Config.Groups[GroupID].ConnectMessage;
                    message = message.Replace("{playername}", player.PlayerName);

                    Server.PrintToChatAll(Models.PluginMessageFormatter.FormatColor(message));
                }
            }

        });

        return HookResult.Continue;
    }

    private void OnClientDisconnect(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (!PlayerManager.IsValid(player) || !PlayerCache.ContainsKey(player)) return;

        int GroupId = PlayerCache[player].GroupId;
        if (GroupId != -1)
        {
            if (Config!.Groups[GroupId].DisconnectMessage != string.Empty)
            {
                string message = Config.Groups[GroupId].DisconnectMessage;
                message = message.Replace("{playername}", player.PlayerName);

                Server.PrintToChatAll(Models.PluginMessageFormatter.FormatColor(message));
            }
        }

        PlayerCache.Remove(player);
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        int currentRound = GetTeamScore(CsTeam.CounterTerrorist) + GetTeamScore(CsTeam.Terrorist);

        if (randomVipManager!.IsRound(currentRound))
        {
            randomVipManager!.ProcessRound(Localizer);
        }

        return HookResult.Continue;
    }



    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) || player.IsBot || !PlayerCache.ContainsKey(player))
        {
            return HookResult.Continue;
        }

        if (nightVipManager!.IsNightVipTime() && nightVipManager.PlayerQualifies(player))
        {


            nightVipManager.GiveNightVip(player);
        }

        int playerGroupID = PlayerCache[player].GroupId = groupManager!.GetPlayerGroup(player);
        if (playerGroupID != -1)
        {
            AddTimer(1.0f, () =>
            {
                PlayerSpawnn_TimerGive(player, playerGroupID);
            });
        }


        return HookResult.Continue;
    }

    private void PlayerSpawnn_TimerGive(CCSPlayerController player, int playerGroupID)
    {
        if (!PlayerManager.IsValid(player) || !player.PawnIsAlive) return;

        Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];

        PlayerManager.SetHealth(player, playerGroup.SpawnHP);

        if (!(IsPistolRound() && !playerGroup.GiveSpawnMoneyPistolRound))
            PlayerManager.AddMoney(player, playerGroup.SpawnMoney);

        PlayerManager.SetArmor(player, playerGroup.SpawnArmor);

        if (playerGroup.SpawnHelmet) { player.GiveNamedItem(CsItem.KevlarHelmet); }


        if (playerGroup.SpawnDefuser && player.TeamNum is (int)CsTeam.CounterTerrorist && !player.PawnHasDefuser)
        {
            player.GiveNamedItem("item_defuser");
        }


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



        AddTimer(0.5f, () =>
        {
            PlayerManager.RefreshUI(player, player.PlayerPawn!.Value!.WeaponServices!.ActiveWeapon!.Value!.As<CCSWeaponBase>().VData!.GearSlot);
        });
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!PlayerManager.IsValid(player) || player.IsBot)
            {
                continue;
            }

            if (!PlayerCache.ContainsKey(player))
            {
                return HookResult.Continue;
            }

            int playerGroupID = PlayerCache[player].GroupId;
            if (playerGroupID != -1)
            {
                Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];

                PlayerManager.AddMoney(player, playerGroup.RoundWonMoney);
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController attacker = @event.Attacker;

        if (!PlayerManager.IsValid(attacker) ||
            attacker.IsBot ||
            !PlayerCache.ContainsKey(attacker)) return HookResult.Continue;



        int playerGroupID = PlayerCache[attacker].GroupId;
        if (playerGroupID == -1) return HookResult.Continue;

        Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];

        if (@event.Headshot)
        {
            PlayerManager.AddMoney(attacker, playerGroup.HeadshotKillMoney);

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup.HeadshotKillHP, playerGroup.MaxHP);
            PlayerManager.SetHealth(attacker, newHP);
        }
        else
        {
            attacker.InGameMoneyServices!.Account += playerGroup.KillMoney;

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup.KillHP, playerGroup.MaxHP);
            PlayerManager.SetHealth(attacker, newHP);
        }


        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
      player.IsBot ||
      !PlayerCache.ContainsKey(player)) return HookResult.Continue;

        int playerGroupID = PlayerCache[player].GroupId;
        if (playerGroupID != -1)
        {
            Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];
            PlayerManager.AddMoney(player, playerGroup.BombPlantMoney);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;


        if (!PlayerManager.IsValid(player) ||
      player.IsBot ||
      !PlayerCache.ContainsKey(player)) return HookResult.Continue;

        int playerGroupID = PlayerCache[player].GroupId;
        if (playerGroupID != -1)
        {
            Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];
            PlayerManager.AddMoney(player, playerGroup.BombDefuseMoney);



        }

        return HookResult.Continue;
    }


    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
         player.IsBot ||
        !PlayerCache.ContainsKey(player)) return HookResult.Continue;

        int playerGroupID = PlayerCache[player].GroupId;
        if (playerGroupID == -1) return HookResult.Continue;

        Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];
        if (@event.Hitgroup == 0 &&
            !new[] { "inferno", "hegrenade", "knife", "zeus" }.Contains(@event.Weapon) &&
            playerGroup.NoFallDamage)
        {
            PlayerManager.AddHealth(player, @event.DmgHealth);
        }


        return HookResult.Continue;
    }

    private void OnTick(CCSPlayerController player)
    {
        if (!PlayerManager.IsValid(player) ||
        player.IsBot ||
        !PlayerCache.ContainsKey(player)) return;

        Models.PlayerData playerData = PlayerCache[player];
        if (playerData.GroupId == -1) { return; }

        Models.VipGroupData playerGroup = Config!.Groups[playerData.GroupId];
        if (playerGroup.ExtraJumps == 0)
        {
            return;
        }

        CCSPlayerPawn pawn = player.PlayerPawn!.Value!;


        var flags = (PlayerFlags)pawn.Flags;
        var buttons = player.Buttons;

        var lastFlags = PlayerCache[player].LastFlags;
        var lastButtons = PlayerCache[player].LastButtons;

        if ((lastFlags & PlayerFlags.FL_ONGROUND) != 0 &&
             (flags & PlayerFlags.FL_ONGROUND) == 0 &&
             (lastButtons & PlayerButtons.Jump) == 0 &&
             (buttons & PlayerButtons.Jump) != 0)
        {
            PlayerCache[player].JumpsUsed++;
        }
        else if ((flags & PlayerFlags.FL_ONGROUND) != 0)
        {
            PlayerCache[player].JumpsUsed = 0;
        }
        else if ((lastButtons & PlayerButtons.Jump) == 0 &&
            (buttons & PlayerButtons.Jump) != 0 &&
            PlayerCache[player].JumpsUsed <= playerGroup.ExtraJumps)
        {
            PlayerCache[player].JumpsUsed++;

            const float Z_VELOCITY = (float)250.0;
            pawn.AbsVelocity.Z = Z_VELOCITY;

        }

        PlayerCache[player].LastFlags = flags;
        PlayerCache[player].LastButtons = buttons;
    }
}
