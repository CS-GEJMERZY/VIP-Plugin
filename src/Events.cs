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

            if (playerGroup.Messages.Chat.Connect.Enabled)
            {
                var message = playerGroup.Messages.Chat.Connect.Message.Replace("{playername}", player.PlayerName);
                message = message.Replace("{playername}", player.PlayerName);

                Server.PrintToChatAll($" {MessageFormatter.FormatColor(message)}");

                if (playerGroup.Messages.Chat.Connect.DontBroadcast)
                {
                    info.DontBroadcast = true;
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
            !_playerCache.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int GroupId = playerData.GroupId;
        if (GroupId == -1) return HookResult.Continue;

        var playerGroup = Config!.VIPGroups[GroupId];

        if (playerGroup.Messages.Chat.Disconnect.Enabled)
        {
            var message = playerGroup.Messages.Chat.Disconnect.Message.Replace("{playername}", player.PlayerName);
            Server.PrintToChatAll($" {MessageFormatter.FormatColor(message)}");

            if (playerGroup.Messages.Chat.Disconnect.DontBroadcast)
            {
                info.DontBroadcast = true;
            }
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
        Server.NextFrame(() =>
        {
            if (!PlayerManager.IsValid(player) || !player.PawnIsAlive) return;

            var playerPawn = player.PlayerPawn.Value;

            VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

            playerPawn!.GravityScale = playerGroup.Misc.Gravity;
            playerPawn!.Speed = playerGroup.Misc.Speed;

            PlayerManager.SetHealth(player, playerGroup.Events.Spawn.HpValue, playerGroup.Limits.MaxHp);
            PlayerManager.SetArmor(player, playerGroup.Events.Spawn.ArmorValue);

            if (!IsPistolRound() || playerGroup.Events.Spawn.ExtraMoneyOnPistolRound)
            {
                PlayerManager.AddMoney(player, playerGroup.Events.Spawn.ExtraMoney, playerGroup.Limits.MaxMoney);
            }

            if (playerGroup.Events.Spawn.Helmet &&
                (!IsPistolRound() || playerGroup.Events.Spawn.HelmetOnPistolRound))
            {
                CCSPlayer_ItemServices services = new(playerPawn!.ItemServices!.Handle)
                {
                    HasHelmet = true
                };
            }

            if (playerGroup.Events.Spawn.DefuseKit &&
                player.TeamNum is (int)CsTeam.CounterTerrorist &&
                !player.PawnHasDefuser)
            {
                PlayerManager.GiveItem(player, "item_defuser");
            }


            //if (playerGroup.Events.Spawn.Grenades.StripOnSpawn)
            //{
            //    PlayerManager.StripGrenades(player);
            //}

            if (playerGroup.Events.Spawn.HealthshotOnPistolRound || !IsPistolRound())
            {
                PlayerManager.GiveItem(player, CsItem.Healthshot, playerGroup.Events.Spawn.HealthshotAmount);
            }

            PlayerManager.GiveItem(player, CsItem.Smoke, playerGroup.Events.Spawn.Grenades.Smoke);
            PlayerManager.GiveItem(player, CsItem.HE, playerGroup.Events.Spawn.Grenades.HE);
            PlayerManager.GiveItem(player, CsItem.Flashbang, playerGroup.Events.Spawn.Grenades.Flashbang);
            PlayerManager.GiveItem(player, CsItem.Decoy, playerGroup.Events.Spawn.Grenades.Decoy);

            switch (player.TeamNum)
            {
                case (int)CsTeam.CounterTerrorist:
                    {
                        PlayerManager.GiveItem(player, CsItem.Incendiary, playerGroup.Events.Spawn.Grenades.FireGrenade);
                        break;
                    }
                case (int)CsTeam.Terrorist:
                    {
                        PlayerManager.GiveItem(player, CsItem.Molotov, playerGroup.Events.Spawn.Grenades.FireGrenade);
                        break;
                    }
            }

            if (playerGroup.Events.Spawn.Zeus && (playerGroup.Events.Spawn.ZeusOnPistolRound || !IsPistolRound()))
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
        });

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
                PlayerManager.AddMoney(player, playerGroup.Events.Round.WinMoney, playerGroup.Limits.MaxMoney);
            }
            else
            {
                PlayerManager.AddMoney(player, playerGroup.Events.Round.LoseMoney, playerGroup.Limits.MaxMoney);
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
            PlayerManager.AddMoney(attacker, playerGroup.Events.Kill.HeadshotMoney, playerGroup.Limits.MaxMoney);

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup!.Events!.Kill!.HeadshotHp, playerGroup.Limits.MaxHp);
            PlayerManager.SetHealth(attacker, newHP);
        }
        else
        {
            PlayerManager.AddMoney(attacker, playerGroup.Events.Kill.Money, playerGroup.Limits.MaxMoney);

            int newHP = Math.Min(attacker.PlayerPawn!.Value!.Health + playerGroup!.Events!.Kill.Hp, playerGroup.Limits.MaxHp);
            PlayerManager.SetHealth(attacker, newHP);
        }

        attacker.InGameMoneyServices!.Account += playerGroup.Events.Kill.Money;

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
        PlayerManager.AddMoney(player, playerGroup.Events.Bomb.BombPlantMoney, playerGroup.Limits.MaxMoney);

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
        PlayerManager.AddMoney(player, playerGroup.Events.Bomb.BombDefuseMoney, playerGroup.Limits.MaxMoney);


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
            !_playerCache.TryGetValue(player, out PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        int playerGroupID = playerData.GroupId;

        if (playerGroupID == -1) return HookResult.Continue;
        VipGroupConfig playerGroup = Config!.VIPGroups[playerGroupID];

        if (playerGroup.Misc.NoFallDamage || (playerData.UsingExtraJump && playerGroup.Misc.ExtraJumps.NoFallDamage))
        {
            damageInfo.Damage = 0;
            return HookResult.Stop;
        }

        return HookResult.Continue;
    }

    private void OnTick(CCSPlayerController player)
    {
        if (!PlayerManager.IsValid(player) ||
            player.IsBot ||
             !_playerCache.TryGetValue(player, out Models.PlayerData? playerData)) return;

        if (playerData.GroupId == -1) { return; }

        VipGroupConfig playerGroup = Config!.VIPGroups[playerData.GroupId];
        if (playerGroup.Misc.ExtraJumps.Amount == 0)
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
            playerData.UsingExtraJump = false;
        }
        else if ((flags & PlayerFlags.FL_ONGROUND) != 0)
        {
            playerData.JumpsUsed = 0;
            playerData.UsingExtraJump = false;
        }
        else if ((lastButtons & PlayerButtons.Jump) == 0 &&
            (buttons & PlayerButtons.Jump) != 0 &&
            playerData.JumpsUsed <= playerGroup.Misc.ExtraJumps.Amount)
        {
            playerData.JumpsUsed++;

            float Z_VELOCITY = (float)playerGroup.Misc.ExtraJumps.VelocityZ;
            pawn.AbsVelocity.Z = Z_VELOCITY;
            playerData.UsingExtraJump = true;

        }

        playerData.LastFlags = flags;
        playerData.LastButtons = buttons;
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (entity.DesignerName != "smokegrenade_projectile") return;

        var smokeGrenadeEntity = new CSmokeGrenadeProjectile(entity.Handle);
        if (smokeGrenadeEntity.Handle == IntPtr.Zero) return;

        Server.NextFrame(() =>
        {
            var throwerPawnValue = smokeGrenadeEntity.Thrower.Value;
            if (throwerPawnValue == null) return;

            var throwerValueController = throwerPawnValue!.Controller!.Value!;
            var player = new CCSPlayerController(throwerValueController.Handle);

            if (!PlayerManager.IsValid(player) ||
                !_playerCache.TryGetValue(player, out PlayerData? playerData)) return;

            if (playerData.GroupId == -1) return;
            var playerGroup = Config.VIPGroups[playerData.GroupId];

            if (!playerGroup.Misc.Smoke.Enabled) return;

            switch (playerGroup.Misc.Smoke.Type)
            {
                case SmokeConfigType.Fixed:
                    {
                        var Color = HexToRgb(playerGroup.Misc.Smoke.Color);
                        smokeGrenadeEntity.SmokeColor.X = Color.R;
                        smokeGrenadeEntity.SmokeColor.Y = Color.G;
                        smokeGrenadeEntity.SmokeColor.Z = Color.B;
                        break;
                    }
                case SmokeConfigType.Random:
                    {
                        smokeGrenadeEntity.SmokeColor.X = Random.Shared.NextSingle() * 255.0f;
                        smokeGrenadeEntity.SmokeColor.Y = Random.Shared.NextSingle() * 255.0f;
                        smokeGrenadeEntity.SmokeColor.Z = Random.Shared.NextSingle() * 255.0f;

                        break;
                    }
            }
        });
    }
}
