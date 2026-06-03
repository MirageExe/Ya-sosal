using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Standing;
using Robust.Shared.Prototypes;

namespace Content.Server._Rat.Silicons.Borgs;

/// <summary>
/// Advances Shitmed surgery steps for autonomous cyborgs.
/// </summary>
public sealed class BorgAiSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedSurgerySystem _surgery = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public bool NeedsSurgery(EntityUid patient)
    {
        if (!TryComp<DamageableComponent>(patient, out var damage) || damage.TotalDamage <= 0)
            return false;

        return HasComp<SurgeryTargetComponent>(patient) && HasComp<BodyComponent>(patient);
    }

    public bool PatientTreatable(EntityUid patient)
    {
        if (Deleted(patient) || !TryComp<MobStateComponent>(patient, out var mob))
            return false;

        return !_mobState.IsDead(patient, mob);
    }

    public bool TryLayPatientForSurgery(EntityUid patient)
    {
        if (_standing.IsDown(patient))
            return true;

        if (TryComp<BuckleComponent>(patient, out var buckle) && buckle.Buckled)
            return _standing.IsDown(patient);

        return _standing.Down(patient);
    }

    /// <summary>
    /// Attempts a single valid surgery step on any damaged body part.
    /// </summary>
    public bool TryAdvanceSurgery(EntityUid surgeon, EntityUid patient)
    {
        if (!PatientTreatable(patient) || !NeedsSurgery(patient))
            return false;

        TryLayPatientForSurgery(patient);

        foreach (var part in _body.GetBodyChildren(patient))
        {
            foreach (var surgeryId in _surgery.AllSurgeries)
            {
                if (_surgery.GetSingleton(surgeryId) is not { } surgeryEnt)
                    continue;

                if (_surgery.GetNextStep(patient, part.Id, surgeryEnt) is not { } pair)
                    continue;

                var nextStep = pair.Surgery.Comp!.Steps[pair.Step];

                if (_surgery.TryDoSurgeryStep(patient, part.Id, surgeon, surgeryId, nextStep))
                    return true;
            }
        }

        return false;
    }
}
