using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Client.UserInterface.Systems.Chat;
using System.Linq;

namespace Content.Client._Ratgore.Factions.Commands;

[AnyCommand]
public sealed class SelectFactionCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "selectfaction";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var system = _entitySystemManager.GetEntitySystem<RatFactionSystem>();

        if (args.Length == 0)
        {
            var factions = system.GetAvailableFactions();

            if (factions.Count == 0)
            {
                shell.WriteLine("Нет доступных фракций. Подключитесь к серверу сначала.");
                return;
            }

            shell.WriteLine("Доступные фракции:");
            shell.WriteLine("  none - Без фракции");
            foreach (var faction in factions)
            {
                var wl = faction.IsWhitelisted ? " [WL]" : "";
                shell.WriteLine($"  {faction.Name}{wl} - {faction.Description}");
            }
            shell.WriteLine("\nИспользуйте: selectfaction <название> или selectfaction none");
            return;
        }

        var factionName = string.Join(" ", args);

        if (factionName.ToLower() == "none")
        {
            system.SelectFaction("");
            shell.WriteLine("Фракция сброшена.");
            return;
        }

        var selected = system.GetAvailableFactions().FirstOrDefault(f =>
            f.Name.Equals(factionName, System.StringComparison.OrdinalIgnoreCase));

        if (selected == null)
        {
            shell.WriteLine($"Фракция '{factionName}' не найдена.");
            return;
        }

        system.SelectFaction(selected.Name);
        shell.WriteLine($"Выбрана фракция: {selected.Name}");
    }
}
