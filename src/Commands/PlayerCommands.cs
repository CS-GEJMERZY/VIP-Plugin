using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;

namespace Core;

public partial class Plugin
{
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnTestVipCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var menu = new CenterHtmlMenu(Localizer["testvip.menu.title"], this); 
    }
}

