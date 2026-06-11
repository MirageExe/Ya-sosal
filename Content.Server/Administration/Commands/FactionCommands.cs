using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionCreateCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factioncreate";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        var name = args[0].Trim();
        if (!bool.TryParse(args[1].Trim(), out var isWhitelisted))
        {
            shell.WriteError($"Invalid boolean value: {args[1]}");
            shell.WriteLine(Help);
            return;
        }

        var description = args.Length > 2 ? string.Join(' ', args[2..]) : string.Empty;

        await _db.CreateFaction(name, description, isWhitelisted);
        shell.WriteLine($"Created faction '{name}' (whitelisted: {isWhitelisted}).");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                new[] { "true", "false" },
                "true/false");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionDeleteCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factiondelete";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 1),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factiondelete <id>");
            shell.WriteLine("Use 'factionlist' to see all subfaction IDs");
            return;
        }

        if (!int.TryParse(args[0].Trim(), out var id))
        {
            shell.WriteError($"Invalid ID: {args[0]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see all subfaction IDs");
            return;
        }

        var success = await _db.DeleteFactionById(id);
        if (!success)
        {
            shell.WriteError($"Subfaction with ID {id} not found.");
            return;
        }

        shell.WriteLine($"Deleted subfaction with ID {id}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionListCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factionlist";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 0),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        var factions = await _db.GetAllFactions();
        if (factions.Count == 0)
        {
            shell.WriteLine("No subfactions found.");
            return;
        }

        shell.WriteLine($"{"ID",-5} | {"Name",-30} | {"Whitelisted",-12} | Description");
        shell.WriteLine(new string('-', 100));
        foreach (var faction in factions)
        {
            var whitelisted = faction.IsWhitelisted ? "yes" : "no";
            var desc = faction.Description.Length > 40 ? faction.Description[..37] + "..." : faction.Description;
            shell.WriteLine($"{faction.Id,-5} | {faction.Name,-30} | {whitelisted,-12} | {desc}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionSetManagerCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionsetmanager";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factionsetmanager <player> <faction_id>");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError($"Invalid subfaction ID: {args[1]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError($"Player '{playerName}' not found.");
            return;
        }

        var success = await _db.AddFactionManagerById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError($"Failed to set manager for subfaction ID {factionId}. Subfaction may not exist or player is already a manager.");
            return;
        }

        shell.WriteLine($"Set '{playerName}' as manager of subfaction ID {factionId}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                "Player name");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionRemoveManagerCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionremovemanager";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factionremovemanager <player> <faction_id>");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError($"Invalid subfaction ID: {args[1]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError($"Player '{playerName}' not found.");
            return;
        }

        var success = await _db.RemoveFactionManagerById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError($"Failed to remove manager from subfaction ID {factionId}. Subfaction or manager may not exist.");
            return;
        }

        shell.WriteLine($"Removed '{playerName}' as manager of subfaction ID {factionId}.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                "Player name");
        }

        return CompletionResult.Empty;
    }
}
