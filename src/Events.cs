using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core;

public partial class Plugin
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
            if (!_playerCache.TryGetValue(player, out PlayerData? playerData))
            {
                playerData = new PlayerData();
                _playerCache.Add(player, playerData);
            }

            playerData.GroupId = GroupManager!.GetPlayerGroup(player);

            if (playerData.GroupId == -1) return;

            var playerGroup = Config.VIPGroups[playerData.GroupId];

            if (!string.IsNullOrEmpty(playerGroup.Messages.ConnectChat))
            {
                var message = playerGroup.Messages.ConnectChat.Replace("{playername}", player.PlayerName);
                message = message.Replace("{playername}", player.PlayerName);

                Server.PrintToChatAll(MessageFormatter.FormatColor(message));
            }
        });

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
            !_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int GroupId = playerData.GroupId;
        if (GroupId == -1) return HookResult.Continue;

        var playerGroup = Config!.VIPGroups[GroupId];

        if (playerGroup.Messages.DisconnectChat != string.Empty)
        {
            string message = playerGroup.Messages.DisconnectChat;
            message = message.Replace("{playername}", player.PlayerName);

            Server.PrintToChatAll(Models.MessageFormatter.FormatColor(message));
        }

        _playerCache.Remove(player);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        int currentRound = GetTeamScore(CsTeam.CounterTerrorist) + GetTeamScore(CsTeam.Terrorist);

        if (Config.RandomVip.Enabled &&
            RandomVipManager!.IsRound(currentRound))
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
            !_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        if (Config.NightVip.Enabled &&
            NightVipManager!.IsNightVipTime() &&
            NightVipManager.PlayerQualifies(player))
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

        var playerPawn = player.PlayerPawn.Value;

        VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

        PlayerManager.SetHealth(player, playerGroup.Spawn.HpValue, playerGroup.Limits.MaxHp);


        if (!IsPistolRound() || playerGroup.Spawn.ExtraMoneyOnPistolRound)
        {
            PlayerManager.AddMoney(player, playerGroup.Spawn.ExtraMoney, playerGroup.Limits.MaxMoney);
        }

        PlayerManager.SetArmor(player, playerGroup.Spawn.ArmorValue);

        if (playerGroup.Spawn.Helmet &&
            !(IsPistolRound() && !playerGroup.Spawn.HelmetOnPistolRound))
        {
            CCSPlayer_ItemServices services = new(playerPawn!.ItemServices!.Handle);
            services.HasHelmet = true;
        }

        if (playerGroup.Spawn.DefuseKit &&
            player.TeamNum is (int)CsTeam.CounterTerrorist &&
            !player.PawnHasDefuser)
        {
            PlayerManager.GiveItem(player, "item_defuser");
        }

        if (playerGroup.Grenades.StripOnSpawn)
        {
            PlayerManager.StripGrenades(player);
        }

        PlayerManager.GiveItem(player, CsItem.Smoke, playerGroup.Grenades.Smoke);
        PlayerManager.GiveItem(player, CsItem.HE, playerGroup.Grenades.HE);
        PlayerManager.GiveItem(player, CsItem.Flashbang, playerGroup.Grenades.Flashbang);
        PlayerManager.GiveItem(player, CsItem.Decoy, playerGroup.Grenades.Decoy);

        switch (player.TeamNum)
        {
            case (int)CsTeam.CounterTerrorist:
                {
                    PlayerManager.GiveItem(player, CsItem.Incendiary, playerGroup.Grenades.FireGrenade);
                    break;
                }
            case (int)CsTeam.Terrorist:
                {
                    PlayerManager.GiveItem(player, CsItem.Molotov, playerGroup.Grenades.FireGrenade);
                    break;
                }
        }

        if (playerGroup.Spawn.Zeus && (playerGroup.Spawn.ZeusOnPistolRound || !IsPistolRound()))
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
            if (!hasZeus)
            {
                PlayerManager.GiveItem(player, CsItem.Zeus, 1);
            }
        }

        Utilities.SetStateChanged(playerPawn!, "CBasePlayerPawn", "m_pItemServices");
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!PlayerManager.IsValid(player) ||
                player.IsBot ||
                !player!.PawnIsAlive)
            {
                continue;
            }

            if (!_playerCache.TryGetValue(player, out Models.PlayerData? value))
            {
                return HookResult.Continue;
            }

            int playerGroupID = value.GroupId;
            if (playerGroupID == -1) continue;
            VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

            CsTeam winnerTeam = (CsTeam)(@event.Winner);
            if (player.Team == winnerTeam)
            {
                PlayerManager.AddMoney(player, playerGroup.Events.RoundConfig.WinMoney, playerGroup.Limits.MaxMoney);
            }
            else
            {
                PlayerManager.AddMoney(player, playerGroup.Events.RoundConfig.LoseMoney, playerGroup.Limits.MaxMoney);
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
            !_playerCache.TryGetValue(attacker, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;
        if (playerGroupID == -1)
        {
            return HookResult.Continue;
        }

        VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

        if (@event.Headshot)
        {
            PlayerManager.AddMoney(attacker, playerGroup.Events.KillConfig.HeadshotMoney, playerGroup.Limits.MaxMoney);

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup!.Events!.KillConfig!.HeadshotHp, playerGroup.Limits.MaxHp);
            PlayerManager.SetHealth(attacker, newHP);
        }
        else
        {
            PlayerManager.AddMoney(attacker, playerGroup.Events.KillConfig.Money, playerGroup.Limits.MaxMoney);

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup!.Events!.KillConfig.Hp, playerGroup.Limits.MaxHp);
            PlayerManager.SetHealth(attacker, newHP);
        }

        attacker.InGameMoneyServices!.Account += playerGroup.Events.KillConfig.Money;

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
            !_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;
        if (playerGroupID == -1) return HookResult.Continue;

        VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];
        PlayerManager.AddMoney(player, playerGroup.Events.BombConfig.BombPlantMoney, playerGroup.Limits.MaxMoney);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;


        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
            !_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;
        if (playerGroupID == -1) return HookResult.Continue;

        VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];
        PlayerManager.AddMoney(player, playerGroup.Events.BombConfig.BombDefuseMoney, playerGroup.Limits.MaxMoney);


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
            !_playerCache.TryGetValue(player, out Models.PlayerData? value))
        {
            return HookResult.Continue;
        }

        int playerGroupID = value.GroupId;

        if (playerGroupID == -1) return HookResult.Continue;
        Config.VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

        if (playerGroup.Misc.NoFallDamage)
        {
            damageInfo.Damage = 0;
        }

        return HookResult.Stop;
    }

    private void OnTick(CCSPlayerController player)
    {
        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
             !_playerCache.TryGetValue(player, out Models.PlayerData? playerData)) return;

        if (playerData.GroupId == -1) { return; }

        VipGroupConfig playerGroup = Config!.VIPGroups[playerData.GroupId];
        if (playerGroup.Misc.ExtraJumps.Count == 0)
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
            playerData.JumpsUsed <= playerGroup.Misc.ExtraJumps.Count)
        {
            playerData.JumpsUsed++;

            float Z_VELOCITY = (float)playerGroup.Misc.ExtraJumps.VelocityZ;
            pawn.AbsVelocity.Z = Z_VELOCITY;

        }

        playerData.LastFlags = flags;
        playerData.LastButtons = buttons;
    }
}
