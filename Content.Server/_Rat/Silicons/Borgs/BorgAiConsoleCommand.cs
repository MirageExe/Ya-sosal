using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Server._Rat.Silicons.Borgs;

[AdminCommand(AdminFlags.Fun)]
public sealed class BorgAiOrderCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public BorgAiOrderCommand()
    {
        IoCManager.InjectDependencies(this);
    }

    public string Command => "borgai_order";
    public string Description => "Issue an order to an autonomous cyborg.";
    public string Help => "borgai_order <borgUid> <order> [targetUid|targetName]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Not enough arguments.");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var borgNet) || !_entManager.TryGetEntity(borgNet, out var borg))
        {
            shell.WriteError("Invalid borg entity.");
            return;
        }

        if (!Enum.TryParse<BorgAiOrderType>(args[1], true, out var order))
        {
            shell.WriteError("Invalid order type.");
            return;
        }

        EntityUid? target = null;
        string? name = null;

        if (args.Length >= 3)
        {
            if (NetEntity.TryParse(args[2], out var targetNet) && _entManager.TryGetEntity(targetNet, out var targetEnt))
                target = targetEnt;
            else
                name = args[2];
        }

        var system = _entManager.System<BorgAiCommandSystem>();
        if (!system.TryIssueOrder(borg!.Value, order, target, targetName: name))
            shell.WriteError("Failed to issue order.");
        else
            shell.WriteLine("Order issued.");
    }
}
