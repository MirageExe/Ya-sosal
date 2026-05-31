using System;
using Content.Server.Administration;
using Content.Shared._Rat.YoutubeScreen;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Server._Rat.YoutubeScreen;

[AdminCommand(AdminFlags.Fun)]
public sealed class YoutubeScreenCommand : IConsoleCommand
{
    public string Command => "youtubescreen";
    public string Description => "Set YouTube video on a screen entity.";
    public string Help => "youtubescreen <entityId> <youtube url or id> [play 0|1]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError("Invalid entity id.");
            return;
        }

        var entMan = IoCManager.Resolve<IEntityManager>();
        if (!entMan.TryGetComponent(uid, out YoutubeScreenComponent? comp))
        {
            shell.WriteError("Entity has no YoutubeScreen component.");
            return;
        }

        bool? play = null;
        if (args.Length >= 3)
            play = args[2] == "1" || args[2].Equals("true", StringComparison.OrdinalIgnoreCase);

        var system = entMan.System<YoutubeScreenSystem>();
        if (!system.TrySetVideo((uid, comp), args[1], play))
        {
            shell.WriteError("Could not parse YouTube URL or video id.");
            return;
        }

        shell.WriteLine($"Screen set to '{comp.VideoId}', playing={comp.Playing}.");
    }
}
