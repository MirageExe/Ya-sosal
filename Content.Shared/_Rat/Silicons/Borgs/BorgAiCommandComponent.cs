using Robust.Shared.GameStates;

namespace Content.Shared._Rat.Silicons.Borgs;

/// <summary>
/// Present on a cyborg chassis while an AI brain is active.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgAiCommandComponent : Component
{
    [DataField, AutoNetworkedField]
    public BorgAiOrderType CurrentOrder = BorgAiOrderType.Idle;

    [DataField, AutoNetworkedField]
    public EntityUid? OrderTarget;

    [DataField]
    public float CommandRange = BorgAiBlackboard.DefaultCommandRange;
}
