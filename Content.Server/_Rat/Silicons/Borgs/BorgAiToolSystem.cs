using Content.Server.Silicons.Borgs;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._Rat.Silicons.Borgs;

/// <summary>
/// Locates tools in cyborg module virtual hands.
/// </summary>
public sealed class BorgAiToolSystem : EntitySystem
{
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    /// <summary>
    /// Powers the borg on and materializes module items into virtual hands.
    /// </summary>
    public void PrepareForWork(EntityUid borg)
    {
        if (!TryComp<BorgChassisComponent>(borg, out var chassis))
            return;

        if (HasComp<ItemToggleComponent>(borg))
            _toggle.TryActivate(borg);

        _borg.InstallAllModules(borg, chassis);

        if (TryFindInHands(borg, _ => true, out _))
            return;

        foreach (var module in chassis.ModuleContainer.ContainedEntities)
        {
            if (!HasComp<BorgModuleComponent>(module))
                continue;

            if (HasComp<SelectableBorgModuleComponent>(module))
                _borg.SelectModule(borg, module, chassis);

            if (TryFindInHands(borg, _ => true, out _))
                return;
        }
    }

    public bool TryFindItemWithTag(EntityUid borg, string tagId, out EntityUid item)
    {
        item = default;

        if (!_prototypes.TryIndex<TagPrototype>(tagId, out var tag))
            return false;

        return TryFindItem(borg, ent => _tags.HasTag(ent, tag), out item);
    }

    public bool TryFindItemWithComponent<T>(EntityUid borg, out EntityUid item) where T : IComponent, new()
    {
        PrepareForWork(borg);
        return TryFindItem(borg, ent => HasComp<T>(ent), out item);
    }

    public bool TryFindItem(EntityUid borg, Func<EntityUid, bool> predicate, out EntityUid item)
    {
        item = default;
        PrepareForWork(borg);

        if (TryFindInHands(borg, predicate, out item))
            return true;

        if (!TryComp<BorgChassisComponent>(borg, out var chassis))
            return false;

        foreach (var module in chassis.ModuleContainer.ContainedEntities)
        {
            if (!HasComp<SelectableBorgModuleComponent>(module))
                continue;

            _borg.SelectModule(borg, module, chassis);

            if (TryFindInHands(borg, predicate, out item))
                return true;
        }

        return false;
    }

    private bool TryFindInHands(EntityUid borg, Func<EntityUid, bool> predicate, out EntityUid item)
    {
        item = default;

        if (!TryComp<HandsComponent>(borg, out var hands))
            return false;

        foreach (var held in _hands.EnumerateHeld(borg, hands))
        {
            if (!predicate(held))
                continue;

            item = held;
            return true;
        }

        return false;
    }

    public bool IsInInteractionRange(EntityUid borg, EntityUid target)
    {
        return _transform.InRange(borg, target, SharedInteractionSystem.InteractionRange);
    }
}
