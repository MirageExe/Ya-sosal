// SPDX-License-Identifier: AGPL-3.0-or-later
// Stub for ratgore Shuttles system

using Robust.Shared.GameStates;

namespace Content.Shared._Rat.Shuttles.Components;

/// <summary>
/// Stub component for rat shuttle system.
/// This needs proper implementation when the shuttle system is added.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RatShuttleComponent : Component
{
    [DataField("enabled")]
    public bool Enabled { get; set; } = true;
}
