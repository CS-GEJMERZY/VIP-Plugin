using Core.Config;
using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace Core;

public partial class Plugin
{
    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player!.IsBot ||
            player.IsHLTV)
        {
            return HookResult.Continue;
        }

        AddTimer(1.0f, () =>
        {
            if (!_playerData.TryGetValue(player, out PlayerData? playerData))
            {
                playerData = new PlayerData();
                _playerData.Add(player, playerData);
            }

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
                    Logger.LogError("Error while loading DB player data on join: {message}", ex.ToString());
                }

                await Server.NextFrameAsync(() =>
                {
                    if (playerData.Group == null ||
                        !playerData.Group.Messages.Chat.Connect.Enabled)
                    {
                        return;
                    }

                    var message = playerData.Group.Messages.Chat.Connect.Message.Replace("{playername}", player.PlayerName);
                    message = message.Replace("{playername}", player.PlayerName);

                    Server.PrintToChatAll($" {MessageFormatter.FormatColor(message)}");
                });
            });
        });

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
            !_playerData.TryGetValue(player!, out PlayerData? playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        if (playerData.Group.Messages.Chat.Disconnect.Enabled)
        {
            var message = playerData.Group.Messages.Chat.Disconnect.Message.Replace("{playername}", player!.PlayerName);
            Server.PrintToChatAll($" {MessageFormatter.FormatColor(message)}");
        }

        _playerData.Remove(player!);

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

        for (int i = 0; i < Config.VIPGroups.Count; i++)
        {
            var group = Config.VIPGroups[i];

            if (i < _healthRegenTimers.Count)
            {
                if (_healthRegenTimers[i] != null)
                {
                    _healthRegenTimers[i]!.Dispose();
                    _healthRegenTimers[i] = null;
                }

                if (group.Misc.HealthRegen.Enabled)
                {
                    _healthRegenTimers[i] = new Timer(HealthRegenCallback!, group, group.Misc.HealthRegen.Delay * 1000, group.Misc.HealthRegen.Interval * 1000);
                }
            }
            else
            {
                Logger.LogError("Registered {GroupCount} groups, but there's only HealthRegenTimers.Count place for timer. i = {i}", Config.VIPGroups.Count, i);
            }

            if (i < _armorRegenTimers.Count)
            {
                if (_armorRegenTimers[i] != null)
                {
                    _armorRegenTimers[i]!.Dispose();
                    _armorRegenTimers[i] = null;
                }

                if (group.Misc.ArmorRegen.Enabled)
                {
                    _armorRegenTimers[i] = new Timer(ArmorRegenCallback!, group, group.Misc.ArmorRegen.Delay * 1000, group.Misc.ArmorRegen.Interval * 1000);
                }
            }
            else
            {
                Logger.LogError("Registered {GroupCount} groups, but there's only HealthRegenTimers.Count place for timer. i = {i}", Config.VIPGroups.Count, i);
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player!.IsBot ||
            !_playerData.TryGetValue(player, out Models.PlayerData? playerData))
        {
            return HookResult.Continue;
        }

        bool qualifiesForNightVip = Config.NightVip.Enabled &&
                                    NightVipManager!.IsNightVipTime() &&
                                    NightVipManager.PlayerQualifies(player);

        playerData.LoadBaseGroup(player, GroupManager!);

        if (DatabaseVipsEnabled &&
            Config.Settings.DatabaseVips.ReloadPlayersOnSpawn)
        {
            Task.Run(async () =>
            {
                try
                {

                    await playerData.LoadDatabaseVipDataAsync(player, GroupManager!, DatabaseManager!);

                }
                catch (Exception ex)
                {
                    Logger.LogError("Error while realoding player on spawn: {error}", ex.ToString());
                }
            });
        }

        if (qualifiesForNightVip)
        {
            NightVipManager!.GiveNightVip(player);
        }

        var playerGroup = playerData.Group;
        if (playerGroup == null)
        {
            return HookResult.Continue;
        }

        Server.NextFrame(() =>
        {
            CCSPlayerPawn? playerPawn = player?.PlayerPawn?.Value;

            if (!PlayerManager.IsValid(player) ||
                !player!.PawnIsAlive ||
                playerGroup == null ||
                playerPawn == null ||
                playerPawn.ItemServices == null ||
                playerPawn.WeaponServices == null)
            {
                return;
            }

            CCSPlayer_ItemServices itemServices = new(playerPawn.ItemServices!.Handle);

            // Helmet
            if (playerGroup.Events.Spawn.Helmet &&
                (!IsPistolRound() || playerGroup.Events.Spawn.HelmetOnPistolRound))
            {
                itemServices.HasHelmet = true;
            }

            // Defuser
            if (playerGroup.Events.Spawn.DefuseKit &&
                player.TeamNum == (int)CsTeam.CounterTerrorist &&
                !player.PawnHasDefuser)
            {
                itemServices.HasDefuser = true;
            }

            // Gravity and velocity
            playerPawn.GravityScale = playerGroup.Misc.Gravity;
            playerPawn.VelocityModifier = playerGroup.Misc.Speed;

            // health and armor
            PlayerManager.SetHealth(player, playerGroup.Events.Spawn.HpValue, playerGroup.Limits.MaxHp);
            PlayerManager.SetArmor(player, playerGroup.Events.Spawn.ArmorValue);

            // money
            if (!IsPistolRound() || playerGroup.Events.Spawn.ExtraMoneyOnPistolRound)
            {
                PlayerManager.AddMoney(player, playerGroup.Events.Spawn.ExtraMoney, playerGroup.Limits.MaxMoney);
            }

            // healthshot
            if (playerGroup.Events.Spawn.HealthshotOnPistolRound || !IsPistolRound())
            {
                PlayerManager.GiveItem(player, CsItem.Healthshot, playerGroup.Events.Spawn.HealthshotAmount);
            }

            // grenades
            PlayerManager.GiveItem(player, CsItem.Smoke, playerGroup.Events.Spawn.Grenades.Smoke);
            PlayerManager.GiveItem(player, CsItem.HE, playerGroup.Events.Spawn.Grenades.HE);
            PlayerManager.GiveItem(player, CsItem.Flashbang, playerGroup.Events.Spawn.Grenades.Flashbang);
            PlayerManager.GiveItem(player, CsItem.Decoy, playerGroup.Events.Spawn.Grenades.Decoy);

            // fire grenade
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

            // zeus
            if (playerGroup.Events.Spawn.Zeus && (playerGroup.Events.Spawn.ZeusOnPistolRound || !IsPistolRound()))
            {
                if (!PlayerManager.HasWeapon(playerPawn, "weapon_taser"))
                {
                    PlayerManager.GiveItem(player, CsItem.Zeus, 1);
                }
            }

            Utilities.SetStateChanged(playerPawn!, "CBasePlayerPawn", "m_pItemServices");
        });

        return HookResult.Continue;
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

            if (!_playerData.TryGetValue(player, out PlayerData? playerData) ||
                playerData.Group == null)
            {
                return HookResult.Continue;
            }

            VipGroupConfig playerGroup = playerData.Group;

            CsTeam winnerTeam = (CsTeam)@event.Winner;

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
        CCSPlayerController? attacker = @event.Attacker;

        if (!PlayerManager.IsValid(attacker) ||
            attacker!.IsBot ||
            !_playerData.TryGetValue(attacker, out PlayerData? playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        VipGroupConfig playerGroup = playerData.Group;

        if (@event.Headshot)
        {
            PlayerManager.AddMoney(attacker, playerGroup.Events.Kill.HeadshotMoney, playerGroup.Limits.MaxMoney);

            PlayerManager.AddHealth(attacker, playerData.Group.Events.Kill.HeadshotHp, playerGroup.Limits.MaxHp);
        }
        else
        {
            PlayerManager.AddMoney(attacker, playerGroup.Events.Kill.Money, playerGroup.Limits.MaxMoney);

            PlayerManager.AddHealth(attacker, playerData.Group.Events.Kill.Hp, playerGroup.Limits.MaxHp);
        }

        attacker.InGameMoneyServices!.Account += playerGroup.Events.Kill.Money;

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player!.IsBot ||
            !_playerData.TryGetValue(player, out PlayerData? playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        PlayerManager.AddMoney(player, playerData.Group.Events.Bomb.BombPlantMoney, playerData.Group.Limits.MaxMoney);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (!PlayerManager.IsValid(player) ||
            player!.IsBot ||
            !_playerData.TryGetValue(player, out Models.PlayerData? playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        PlayerManager.AddMoney(player, playerData.Group.Events.Bomb.BombDefuseMoney, playerData.Group.Limits.MaxMoney);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombBeginDefuse(EventBombBegindefuse @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
            !_playerData.TryGetValue(player!, out var playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        var gameRules = GetGamerules();
        if (gameRules == null)
        {
            return HookResult.Continue;
        }

        var time = Server.CurrentTime - gameRules.RoundStartTime;

        if (playerData.Group.Misc.FastDefuse.Enabled &&
            time >= playerData.Group.Misc.FastDefuse.TimeAfterRoundStart)
        {
            Server.NextFrame(() =>
            {
                var bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").First();
                if (bomb == null || !bomb.IsValid)
                {
                    Logger.LogWarning("Bomb entity is null or is not valid");
                    return;
                }

                float CountDown = bomb.DefuseCountDown;
                float remainingTime = CountDown - Server.CurrentTime;

                float modifiedTime = remainingTime * playerData.Group.Misc.FastDefuse.Modifier;

                bomb.DefuseCountDown = modifiedTime + Server.CurrentTime;
                player!.PlayerPawn!.Value!.ProgressBarDuration = (int)float.Ceiling(modifiedTime);
            });
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombBeginPlant(EventBombBeginplant @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!PlayerManager.IsValid(player) ||
            !_playerData.TryGetValue(player!, out var playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        var gameRules = GetGamerules();
        if (gameRules == null)
        {
            return HookResult.Continue;
        }

        var time = Server.CurrentTime - gameRules.RoundStartTime;

        if (playerData.Group.Misc.FastPlant.Enabled &&
            time >= playerData.Group.Misc.FastPlant.TimeAfterRoundStart)
        {
            var playerPawn = player!.PlayerPawn!.Value;

            var weapon = playerPawn!.WeaponServices!.ActiveWeapon;

            if (weapon == null ||
                weapon.Value == null ||
                weapon!.Value!.DesignerName != "weapon_c4")
            {
                return HookResult.Continue;
            }

            var c4 = new CC4(weapon!.Value!.Handle);
            if (c4 == null ||
                !c4.IsValid)
            {
                Logger.LogError("c4 entity is null or is not valid");
                return HookResult.Continue;
            }

            float remainingTime = c4.ArmedTime - Server.CurrentTime;

            float modifiedTime = remainingTime * playerData.Group.Misc.FastPlant.Modifier;
            c4.ArmedTime = modifiedTime + Server.CurrentTime;
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
            !_playerData.TryGetValue(player, out PlayerData? playerData) ||
            playerData.Group == null)
        {
            return HookResult.Continue;
        }

        if (playerData.Group.Misc.NoFallDamage ||
            (playerData.UsingExtraJump && playerData.Group.Misc.ExtraJumps.NoFallDamage))
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
            player.IsHLTV ||
            !player.PawnIsAlive ||
             !_playerData.TryGetValue(player, out PlayerData? playerData))
        {
            return;
        }

        if (playerData.Group == null)
        {
            return;
        }

        CCSPlayerPawn pawn = player.PlayerPawn!.Value!;

        var flags = (PlayerFlags)pawn.Flags;
        var buttons = player.Buttons;

        var lastFlags = playerData.LastFlags;
        var lastButtons = playerData.LastButtons;

        // bhop
        if (playerData.Group.Misc.Bhop.Enabled)
        {
            if ((buttons & PlayerButtons.Jump) != 0 &&
                (flags & PlayerFlags.FL_ONGROUND) != 0 &&
                (pawn.MoveType & MoveType_t.MOVETYPE_LADDER) == 0)
            {
                pawn.AbsVelocity.Z = playerData.Group.Misc.Bhop.VelocityZ;
            }
        }

        // extrra jumps
        if (playerData.Group.Misc.ExtraJumps.Amount > 0)
        {
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
                playerData.JumpsUsed <= playerData.Group.Misc.ExtraJumps.Amount)
            {
                playerData.JumpsUsed++;

                pawn.AbsVelocity.Z = playerData.Group.Misc.ExtraJumps.VelocityZ;
                playerData.UsingExtraJump = true;

            }
        }

        playerData.LastFlags = flags;
        playerData.LastButtons = buttons;
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (entity.DesignerName != "smokegrenade_projectile")
        {
            return;
        }

        var smokeGrenadeEntity = new CSmokeGrenadeProjectile(entity.Handle);
        if (smokeGrenadeEntity.Handle == IntPtr.Zero)
        {
            return;
        }

        Server.NextFrame(() =>
        {
            var throwerPawnValue = smokeGrenadeEntity.Thrower.Value;
            if (throwerPawnValue == null)
            {
                return;
            }

            var throwerValueController = throwerPawnValue!.Controller!.Value!;
            var player = new CCSPlayerController(throwerValueController.Handle);

            if (!PlayerManager.IsValid(player) ||
                !_playerData.TryGetValue(player, out PlayerData? playerData))
            {
                return;
            }

            if (playerData.Group == null ||
                !playerData.Group.Misc.Smoke.Enabled)
            {
                return;
            }

            switch (playerData.Group.Misc.Smoke.Type)
            {
                case SmokeConfigType.Fixed:
                    {
                        var Color = HexToRgb(playerData.Group.Misc.Smoke.Color);
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
