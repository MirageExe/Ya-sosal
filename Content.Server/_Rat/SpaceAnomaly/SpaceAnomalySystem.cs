using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Sprite;
using Content.Shared._Rat.SpaceAnomaly;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Rat.SpaceAnomaly;

public sealed class SpaceAnomalySystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ScaleVisualsSystem _scaleVisuals = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly EntProtoId[] AnomalyPool =
    {
        "RatSpaceAnomalyGravity",
        "RatSpaceAnomalyBluespace",
        "RatSpaceAnomalyPyro",
        "RatSpaceAnomalyElectric",
        "RatSpaceAnomalyIce",
        "RatSpaceAnomalyFlesh",
        "RatSpaceAnomalyShadow",
        "RatSpaceAnomalyLiquid",
        "RatSpaceAnomalyFlora",
    };

    private TimeSpan _nextSpawn;

    public override void Initialize()
    {
        base.Initialize();
        _nextSpawn = _timing.CurTime + TimeSpan.FromMinutes(8);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        DespawnExpired();

        if (_timing.CurTime < _nextSpawn)
            return;

        _nextSpawn = _timing.CurTime + TimeSpan.FromMinutes(_random.Next(6, 14));

        if (_random.Prob(0.65f))
            TrySpawnRandom();
    }

    public bool TrySpawnRandom(bool fromEvent = false)
    {
        var map = _gameTicker.DefaultMap;
        Vector2 coords;

        for (var i = 0; i < 30; i++)
        {
            coords = _random.NextVector2Box(-2800f, -2800f, 2800f, 2800f);
            if (IsFarFromGrids(coords, map))
                return TrySpawnAt(new MapCoordinates(coords, map), PickPrototype(), fromEvent: fromEvent);
        }

        return false;
    }

    public bool TrySpawnAt(MapCoordinates coords, EntProtoId prototype, Vector2? scale = null, bool fromEvent = false)
    {
        var ent = Spawn(prototype, coords);
        if (!TryComp<SpaceAnomalyComponent>(ent, out var space))
            return false;

        var finalScale = scale ?? space.Scale;
        _scaleVisuals.SetSpriteScale(ent, finalScale);

        space.FromEvent = fromEvent;
        space.DespawnTime = fromEvent
            ? _timing.CurTime + TimeSpan.FromMinutes(30)
            : _timing.CurTime + TimeSpan.FromMinutes(_random.Next(12, 25));
        space.NextBehaviorPulse = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(40, 90));
        Dirty(ent, space);

        var mapCoords = _transform.GetMapCoordinates(ent);
        _chat.DispatchGlobalAnnouncement(
            Loc.GetString(fromEvent ? "space-anomaly-event-spawn-announcement" : "space-anomaly-spawn-announcement",
                ("x", (int) mapCoords.Position.X),
                ("y", (int) mapCoords.Position.Y)),
            colorOverride: Color.MediumPurple);

        return true;
    }

    private void DespawnExpired()
    {
        var query = EntityQueryEnumerator<SpaceAnomalyComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.DespawnTime)
                continue;

            if (comp.FromEvent)
            {
                _chat.DispatchGlobalAnnouncement(
                    Loc.GetString("space-anomaly-event-despawn-announcement"),
                    colorOverride: Color.DarkSlateGray);
            }

            QueueDel(uid);
        }
    }

    private bool IsFarFromGrids(Vector2 coords, MapId mapId)
    {
        var mapCoords = new MapCoordinates(coords, mapId);
        return _lookup.GetEntitiesInRange<MapGridComponent>(mapCoords, 180f).Count == 0;
    }

    private EntProtoId PickPrototype() => _random.Pick(AnomalyPool);
}
