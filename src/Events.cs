using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Plugin.Managers;

namespace Plugin;

public partial class VipPlugin
{
    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
            player.IsHLTV)
        {
            return HookResult.Continue;
        }

        AddTimer(1.0f, () =>
        {
            if (!PlayerCache.TryGetValue(player, out Models.PlayerData? playerData))
            {
                playerData = new Models.PlayerData();
                PlayerCache.Add(player, playerData);
            }

            playerData.GroupId = GroupManager!.GetPlayerGroup(player);

            int GroupID = playerData.GroupId;
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

    [GameEventHandler]
    public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
            !PlayerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int GroupId = playerData.GroupId;
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

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        int currentRound = GetTeamScore(CsTeam.CounterTerrorist) + GetTeamScore(CsTeam.Terrorist);

        if (RandomVipManager!.IsRound(currentRound))
        {
            RandomVipManager!.ProcessRound(Localizer);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
            !PlayerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        if (NightVipManager!.IsNightVipTime() && NightVipManager.PlayerQualifies(player))
        {
            NightVipManager.GiveNightVip(player);
        }

        int playerGroupID = playerData.GroupId = GroupManager!.GetPlayerGroup(player);
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

        if (playerGroup.SpawnHelmet &&
            !(IsPistolRound() && !playerGroup.GiveSpawnHelmetPistolRound))
        {
            player.GiveNamedItem(CsItem.KevlarHelmet);
        }

        if (playerGroup.SpawnDefuser &&
            player.TeamNum is (int)CsTeam.CounterTerrorist &&
            !player.PawnHasDefuser)
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

        if (playerGroup.Zeus)
        {
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

            if (!hasZeus) player.GiveNamedItem(CsItem.Zeus);
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

            if (!PlayerCache.TryGetValue(player, out Models.PlayerData? value))
            {
                return HookResult.Continue;
            }

            int playerGroupID = value.GroupId;
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
            !PlayerCache.TryGetValue(attacker, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }



        int playerGroupID = playerData.GroupId;
        if (playerGroupID == -1)
        {
            return HookResult.Continue;
        }

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
            !PlayerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;
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
            !PlayerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;
        if (playerGroupID != -1)
        {
            Models.VipGroupData playerGroup = Config!.Groups[playerGroupID];
            PlayerManager.AddMoney(player, playerGroup.BombDefuseMoney);
        }

        return HookResult.Continue;
    }


    private HookResult OnTakeDamage(DynamicHook h)
    {
        var entity = h.GetParam<CEntityInstance>(0);
        var damageInfo = h.GetParam<CTakeDamageInfo>(1);

        if (damageInfo.BitsDamageType != (int)DamageTypes_t.DMG_FALL)
        {
            return HookResult.Continue;
        }

        if (entity.DesignerName != "player")
        {
            return HookResult.Continue;
        }

        var player = new CCSPlayerController(pointer: new CCSPlayerPawn(pointer: entity.Handle)!.Controller!.Value!.Handle);

        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
            !PlayerCache.TryGetValue(player, out Models.PlayerData? value))
        {
            return HookResult.Continue;
        }

        int playerGroupID = value.GroupId;
        if (playerGroupID == -1 || !Config!.Groups[playerGroupID].NoFallDamage)
        {
            return HookResult.Continue;
        }

        damageInfo.Damage = 0;

        return HookResult.Stop;
    }

    private void OnTick(CCSPlayerController player)
    {
        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
             !PlayerCache.TryGetValue(player, out Models.PlayerData? playerData)) return;

        if (playerData.GroupId == -1) { return; }

        Models.VipGroupData playerGroup = Config!.Groups[playerData.GroupId];
        if (playerGroup.ExtraJumps == 0)
        {
            return;
        }

        CCSPlayerPawn pawn = player.PlayerPawn!.Value!;


        var flags = (PlayerFlags)pawn.Flags;
        var buttons = player.Buttons;

        var lastFlags = playerData.LastFlags;
        var lastButtons = playerData.LastButtons;

        if ((lastFlags & PlayerFlags.FL_ONGROUND) != 0 &&
             (flags & PlayerFlags.FL_ONGROUND) == 0 &&
             (lastButtons & PlayerButtons.Jump) == 0 &&
             (buttons & PlayerButtons.Jump) != 0)
        {
            playerData.JumpsUsed++;
        }
        else if ((flags & PlayerFlags.FL_ONGROUND) != 0)
        {
            playerData.JumpsUsed = 0;
        }
        else if ((lastButtons & PlayerButtons.Jump) == 0 &&
            (buttons & PlayerButtons.Jump) != 0 &&
            playerData.JumpsUsed <= playerGroup.ExtraJumps)
        {
            playerData.JumpsUsed++;

            const float Z_VELOCITY = (float)250.0;
            pawn.AbsVelocity.Z = Z_VELOCITY;

        }

        playerData.LastFlags = flags;
        playerData.LastButtons = buttons;
    }
}
