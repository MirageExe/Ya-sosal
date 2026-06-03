using Content.Server._Rat.Silicons.Borgs;
using Content.Server.Chat.Systems;
using Content.Server.Medical.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Silicons.Bots;
using Content.Shared._Rat.Silicons.Borgs;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server._Rat.NPC.HTN.Borg;

public sealed partial class BorgHealOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BorgAiToolSystem _tools = default!;
    private BorgAiSurgerySystem _surgeryAi = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedHandsSystem _hands = default!;
    private SharedSolutionContainerSystem _solutions = default!;
    private ChatSystem _chat = default!;

    [DataField(required: true)]
    public string TargetKey = BorgAiBlackboard.Target;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _tools = sysManager.GetEntitySystem<BorgAiToolSystem>();
        _surgeryAi = sysManager.GetEntitySystem<BorgAiSurgerySystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _hands = sysManager.GetEntitySystem<SharedHandsSystem>();
        _solutions = sysManager.GetEntitySystem<SharedSolutionContainerSystem>();
        _chat = sysManager.GetEntitySystem<ChatSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || _entManager.Deleted(target)
            || !_surgeryAi.PatientTreatable(target))
        {
            return HTNOperatorStatus.Failed;
        }

        _tools.PrepareForWork(owner);

        if (!_tools.IsInInteractionRange(owner, target))
            return HTNOperatorStatus.Continuing;

        if (_surgeryAi.NeedsSurgery(target) && _surgeryAi.TryAdvanceSurgery(owner, target))
            return HTNOperatorStatus.Continuing;

        if (_entManager.TryGetComponent<DamageableComponent>(target, out var damage) && damage.TotalDamage <= 0)
            return HTNOperatorStatus.Finished;

        var coords = _entManager.GetComponent<TransformComponent>(target).Coordinates;

        if (_entManager.TryGetComponent<HandsComponent>(owner, out var hands))
        {
            foreach (var held in _hands.EnumerateHeld(owner, hands))
            {
                if (!_entManager.HasComponent<HealingComponent>(held))
                    continue;

                var ev = new AfterInteractEvent(owner, held, target, coords, true);
                _entManager.EventBus.RaiseLocalEvent(held, ev);
                if (ev.Handled)
                    return HTNOperatorStatus.Continuing;
            }
        }

        if (TryMedibotInject(owner, target))
            return HTNOperatorStatus.Continuing;

        return HTNOperatorStatus.Failed;
    }

    private bool TryMedibotInject(EntityUid owner, EntityUid target)
    {
        if (!_entManager.TryGetComponent<DamageableComponent>(target, out var damage)
            || !_entManager.TryGetComponent<MobStateComponent>(target, out var mobState)
            || !_solutions.TryGetInjectableSolution(target, out var injectable, out _))
        {
            return false;
        }

        if (!_interaction.InRangeUnobstructed(owner, target))
            return false;

        var total = damage.TotalDamage;
        var treatment = mobState.CurrentState switch
        {
            MobState.Critical => new MedibotTreatment
            {
                Reagent = "Inaprovaline",
                Quantity = 20,
                MinDamage = 1,
                MaxDamage = 200,
            },
            _ => new MedibotTreatment
            {
                Reagent = "Tricordrazine",
                Quantity = 30,
                MinDamage = 1,
                MaxDamage = 200,
            },
        };

        if (!treatment.IsValid(total))
            return false;

        _solutions.TryAddReagent(injectable.Value, treatment.Reagent, treatment.Quantity, out _);
        _chat.TrySendInGameICMessage(owner, Loc.GetString("borg-ai-heal-inject"), InGameICChatType.Speak, hideChat: true, hideLog: true);
        return true;
    }
}
