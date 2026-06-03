using Robust.Shared.GameStates;

namespace Content.Shared._Rat.Silicons.Borgs;

/// <summary>
/// Marks a brain implant that runs autonomous HTN instead of a player mind.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BorgAiBrainComponent : Component;
