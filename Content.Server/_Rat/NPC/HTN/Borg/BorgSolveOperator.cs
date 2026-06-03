using Content.Server._Rat.Silicons.Borgs;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;
using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Silicons.Bots;
using Content.Shared.Tag;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.Prototypes;

namespace Content.Server._Rat.NPC.HTN.Borg;

/// <summary>
/// Repairs damaged structures/silicons (weldbot-style) for the Solve order.
/// </summary>
public sealed partial class BorgSolveOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    private BorgAiToolSystem _tools = default!;
    private WeldbotSystem _weldbot = default!;
    private DamageableSystem _damageable = default!;
    private TagSystem _tags = default!;
    private SharedInteractionSystem _interaction = default!;

    [DataField(required: true)]
    public string TargetKey = BorgAiBlackboard.Target;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _tools = sysManager.GetEntitySystem<BorgAiToolSystem>();
        _weldbot = sysManager.GetEntitySystem<WeldbotSystem>();
        _damageable = sysManager.GetEntitySystem<DamageableSystem>();
        _tags = sysManager.GetEntitySystem<TagSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || _entManager.Deleted(target)
            || !_entManager.TryGetComponent<DamageableComponent>(target, out var damage))
        {
            return HTNOperatorStatus.Failed;
        }

        _tools.PrepareForWork(owner);

        if (!_tools.IsInInteractionRange(owner, target))
            return HTNOperatorStatus.Continuing;

        if (!_tools.TryFindItemWithTag(owner, "WeldingTool", out var welder))
            return HTNOperatorStatus.Continuing;

        if (!_interaction.InRangeUnobstructed(owner, target))
            return HTNOperatorStatus.Continuing;

        var tagSilicon = _prototypes.Index<TagPrototype>(WeldbotWeldOperator.SiliconTag);
        var tagStructure = _prototypes.Index<TagPrototype>(WeldbotWeldOperator.WeldotFixableStructureTag);

        if (!_entManager.TryGetComponent<TagComponent>(target, out var tagComp))
            return HTNOperatorStatus.Failed;

        var isSilicon = _tags.HasTag(tagComp, tagSilicon);
        var isStructure = _tags.HasTag(tagComp, tagStructure);
        var emagged = _entManager.HasComponent<EmaggedComponent>(owner);

        if (!isSilicon && !isStructure)
            return HTNOperatorStatus.Failed;

        if (isSilicon && damage.DamagePerGroup.TryGetValue("Brute", out var brute) && brute <= 0 && !emagged)
            return HTNOperatorStatus.Finished;

        if (isStructure && damage.TotalDamage <= 0)
            return HTNOperatorStatus.Finished;

        _damageable.TryChangeDamage(target, isSilicon
            ? new DamageSpecifier { DamageDict = { ["Brute"] = -WeldbotWeldOperator.SiliconRepairAmount } }
            : new DamageSpecifier { DamageDict = { ["Structural"] = -WeldbotWeldOperator.StructureRepairAmount } });

        return HTNOperatorStatus.Finished;
    }
}
