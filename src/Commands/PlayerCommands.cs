using Core.Managers;
using Core.Models;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;

namespace Core;

public partial class Plugin
{
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnTestVipCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerManager.IsValid(player))
        {
            return;
        }

        if (!playerCache.TryGetValue(player!, out var playerData))
        {
            player!.PrintToChat($"{PluginPrefix}{Localizer["not_registered"]}");
            return;
        }

        if (playerData.TestVipData.ActiveTestVip != null)
        {
            TestVipData activeTestVip = playerData.TestVipData.ActiveTestVip!;
            player!.PrintToChat($"{PluginPrefix}{Localizer["testvip.command.already_active"]}");

            if (activeTestVip.Mode == TestVipMode.Playtime)
            {
                player!.PrintToChat($"{PluginPrefix}{Localizer["testvip.command.active_end.playtime", activeTestVip.TimeLeft!]}");
            }
            else
            {
                player!.PrintToChat($"{PluginPrefix}{Localizer["testvip.command.active_end.fixed", activeTestVip.End!]}");
            }

            return;
        }

        if (playerData.TestVipData.UsedCount >= Config.TestVip.Plan.MaxUses)
        {
            player!.PrintToChat($"{PluginPrefix}{Localizer["testvip.command.max_uses", Config.TestVip.Plan.MaxUses]}");
            return;
        }

        if (playerData.TestVipData.LastEndTime != null)
        {
            TimeSpan delta = DateTime.UtcNow - (DateTime)playerData.TestVipData.LastEndTime;
            int timeLeft = Config.TestVip.Plan.Delay - (int)delta.TotalMinutes;
            if (timeLeft > 0)
            {
                player!.PrintToChat($"{PluginPrefix}{Localizer["testvip.command.max_uses", Config.TestVip.Plan.MaxUses]}");
                return;
            }
        }

        var menu = new CenterHtmlMenu(Localizer["testvip.menu.title"], this);

        menu.AddMenuOption(Localizer["testvip.menu.redeem"], (Player, option) => TestVipMenuRedeemHandler(Player));
        menu.Open(player!);
    }

    private void TestVipMenuRedeemHandler(CCSPlayerController player)
    {
        if (!PlayerManager.IsValid(player))
        {
            return;
        }

        if (!playerCache.TryGetValue(player!, out var playerData))
        {
            player!.PrintToChat($"{PluginPrefix}{Localizer["not_registered"]}");
            return;
        }

        if (!HandleDatabaseCommand(player) ||
            playerData.TestVipData.ActiveTestVip != null)
        {
            return;
        }

        var data = new TestVipData()
        {
            PlayerId = playerData.DatabaseData.Id,
            Mode = Config.TestVip.Mode,
            Start = DateTime.UtcNow,
            Completed = false
        };

        switch (Config.TestVip.Mode)
        {
            case TestVipMode.Playtime:
                {
                    data.TimeLeft = Config.TestVip.Time;
                    break;
                }
            case TestVipMode.FixedDate:
                {
                    data.End = DateTime.UtcNow.AddMinutes(Config.TestVip.Time);
                    break;
                }
        }

        Task.Run(async () =>
        {
            bool failed = false;

            try
            {
                await DatabaseManager.InsertTestVip(data);
                await playerData.LoadTestVipDataAsync(player, GroupManager!, DatabaseManager!, Config.TestVip);
            }
            catch (Exception ex)
            {
                failed = true;
                Logger.LogError("Error while adding testvip: {error}", ex.ToString());
            }

            await Server.NextFrameAsync(() =>
            {
                if (failed)
                {
                    player.PrintToChat($"{PluginPrefix}{Localizer["testvip.redeem.fail"]}");

                    return;
                }

                player.PrintToChat($"{PluginPrefix}{Localizer["testvip.redeem.success"]}");
            });
        });

    }
}
