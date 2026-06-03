using Content.Server.NPC.HTN;
using Content.Server.Construction.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicon.Components;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.GameObjects;

namespace Content.Server._Rat.Silicons.Borgs;

public static class BorgAiValidation
{
    public static bool CanReceiveOrders(EntityUid borg, IEntityManager entMan)
    {
        return entMan.EntityExists(borg) && entMan.HasComponent<BorgAiCommandComponent>(borg) && entMan.HasComponent<HTNComponent>(borg);
    }

    public static bool IsTargetReachable(EntityUid borg, EntityUid target, SharedTransformSystem xformSys, float extraRange = 0f)
    {
        if (!xformSys.InRange(borg, target, SharedInteractionSystem.InteractionRange + extraRange))
            return false;

        return true;
    }

    public static bool CanDisassemble(EntityUid target, IEntityManager entMan)
    {
        return entMan.HasComponent<ConstructionComponent>(target);
    }

    public static bool CanHeal(EntityUid target, MobStateSystem mobState, IEntityManager entMan)
    {
        if (entMan.HasComponent<SiliconComponent>(target))
            return false;

        if (!entMan.TryGetComponent<MobStateComponent>(target, out var mob))
            return false;

        return !mobState.IsDead(target, mob);
    }

    public static bool NeedsHealing(EntityUid target, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent<DamageableComponent>(target, out var damage))
            return false;

        return damage.TotalDamage > 0;
    }
}
