using Content.Server.Chat.Systems;

using Content.Server.Emp;

using Content.Server.Explosion.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Shared._Rat.SpaceAnomaly;

using Content.Shared.Damage;

using Content.Shared.Eye.Blinding.Systems;

using Content.Shared.Physics;

using Content.Shared.StatusEffect;

using Content.Shared.Throwing;

using Robust.Server.GameObjects;

using Robust.Shared.Map;

using Robust.Shared.Map.Components;

using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;



namespace Content.Server._Rat.SpaceAnomaly;



public sealed class SpaceAnomalyBehaviorSystem : EntitySystem

{

    [Dependency] private readonly ChatSystem _chat = default!;

    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly EmpSystem _emp = default!;

    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    [Dependency] private readonly ExplosionSystem _explosion = default!;

    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly ThrowingSystem _throwing = default!;

    [Dependency] private readonly StatusEffectsSystem _status = default!;



    public override void Update(float frameTime)

    {

        base.Update(frameTime);



        var query = EntityQueryEnumerator<SpaceAnomalyComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var space, out var xform))

        {

            if (space.Studied || _timing.CurTime < space.NextBehaviorPulse)

                continue;



            space.NextBehaviorPulse = _timing.CurTime + TimeSpan.FromSeconds(GetPulseInterval(space.Behavior));

            Dirty(uid, space);



            switch (space.Behavior)

            {

                case SpaceAnomalyBehaviorKind.Gravity:

                    GravityPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Bluespace:

                    BluespacePulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Pyro:

                    PyroPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Electric:

                    ElectricPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Ice:

                    IcePulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Flesh:

                    FleshPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Shadow:

                    ShadowPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Liquid:

                    LiquidPulse(uid, xform);

                    break;

                case SpaceAnomalyBehaviorKind.Flora:

                    FloraPulse(uid, xform);

                    break;

            }

        }

    }



    private static int GetPulseInterval(SpaceAnomalyBehaviorKind behavior) => behavior switch

    {

        SpaceAnomalyBehaviorKind.Bluespace => 110,

        SpaceAnomalyBehaviorKind.Pyro => 75,

        SpaceAnomalyBehaviorKind.Electric => 85,

        SpaceAnomalyBehaviorKind.Ice => 95,

        SpaceAnomalyBehaviorKind.Flesh => 80,

        SpaceAnomalyBehaviorKind.Shadow => 100,

        SpaceAnomalyBehaviorKind.Liquid => 70,

        SpaceAnomalyBehaviorKind.Flora => 88,

        _ => 90,

    };



    private void GravityPulse(EntityUid uid, TransformComponent xform)

    {

        var origin = _transform.GetWorldPosition(xform);

        var lookup = _lookup.GetEntitiesInRange(uid, 45f, LookupFlags.Dynamic | LookupFlags.Sundries);

        var xformQuery = GetEntityQuery<TransformComponent>();

        var physQuery = GetEntityQuery<PhysicsComponent>();



        foreach (var ent in lookup)

        {

            if (ent == uid || !xformQuery.TryGetComponent(ent, out var entXform))

                continue;



            if (physQuery.TryGetComponent(ent, out var phys)

                && (phys.CollisionMask & (int) CollisionGroup.GhostImpassable) != 0)

                continue;



            var dir = origin - _transform.GetWorldPosition(entXform, xformQuery);

            if (dir.LengthSquared() < 0.01f)

                dir = _random.NextVector2();



            _throwing.TryThrow(ent, dir.Normalized() * 12f, 35f, uid, 0);

        }

    }



    private void BluespacePulse(EntityUid uid, TransformComponent xform)

    {

        var mapCoords = _transform.GetMapCoordinates(uid, xform);

        var grids = _lookup.GetEntitiesInRange<MapGridComponent>(mapCoords, 280f);



        foreach (var grid in grids)

        {

            if (!HasComp<ShuttleComponent>(grid.Owner))

                continue;



            for (var i = 0; i < 35; i++)

            {

                var coords = _random.NextVector2Box(-2800f, -2800f, 2800f, 2800f);

                if (!IsOpenSpace(coords, mapCoords.MapId))

                    continue;



                _transform.SetCoordinates(grid.Owner, _transform.ToCoordinates(new MapCoordinates(coords, mapCoords.MapId)));

                _chat.DispatchGlobalAnnouncement(

                    Loc.GetString("space-anomaly-bluespace-shuttle-jump"),

                    colorOverride: Color.Cyan);

                return;

            }

        }

    }



    private void PyroPulse(EntityUid uid, TransformComponent xform)

    {

        var coords = _transform.GetMapCoordinates(uid, xform);

        _explosion.QueueExplosion(coords, "Default", 3f, 3f, 1.5f, 0.4f, 2, false);

        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-pyro-flare"), colorOverride: Color.OrangeRed);

    }



    private void ElectricPulse(EntityUid uid, TransformComponent xform)

    {

        var coords = _transform.GetMapCoordinates(uid, xform);

        _emp.EmpPulse(coords, 28f, 120f, 3.5f);

        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-electric-surge"), colorOverride: Color.Yellow);

    }



    private void IcePulse(EntityUid uid, TransformComponent xform)

    {

        foreach (var ent in _lookup.GetEntitiesInRange(uid, 30f, LookupFlags.Dynamic))

        {

            if (ent == uid || !HasComp<StatusEffectsComponent>(ent))

                continue;



            _status.TryAddStatusEffect(ent, "Slipped", TimeSpan.FromSeconds(4), true);

        }



        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-ice-shear"), colorOverride: Color.LightBlue);

    }



    private void FleshPulse(EntityUid uid, TransformComponent xform)

    {

        var damage = new DamageSpecifier();

        damage.DamageDict.Add("Slash", 12);

        damage.DamageDict.Add("Piercing", 6);



        foreach (var ent in _lookup.GetEntitiesInRange(uid, 28f, LookupFlags.Dynamic))

        {

            if (ent == uid || !HasComp<DamageableComponent>(ent))

                continue;



            _damageable.TryChangeDamage(ent, damage);

        }



        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-flesh-rupture"), colorOverride: Color.IndianRed);

    }



    private void ShadowPulse(EntityUid uid, TransformComponent xform)

    {

        foreach (var ent in _lookup.GetEntitiesInRange(uid, 32f, LookupFlags.Dynamic))

        {

            if (ent == uid || !HasComp<StatusEffectsComponent>(ent))

                continue;



            _status.TryAddStatusEffect(ent, TemporaryBlindnessSystem.BlindingStatusEffect,

                TimeSpan.FromSeconds(3.5), false);

        }



        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-shadow-eclipse"), colorOverride: Color.Purple);

    }



    private void LiquidPulse(EntityUid uid, TransformComponent xform)

    {

        var damage = new DamageSpecifier();

        damage.DamageDict.Add("Poison", 8);



        foreach (var ent in _lookup.GetEntitiesInRange(uid, 26f, LookupFlags.Dynamic))

        {

            if (ent == uid)

                continue;



            if (HasComp<StatusEffectsComponent>(ent))

                _status.TryAddStatusEffect(ent, "Slipped", TimeSpan.FromSeconds(5), true);



            if (HasComp<DamageableComponent>(ent))

                _damageable.TryChangeDamage(ent, damage);

        }



        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-liquid-surge"), colorOverride: Color.Teal);

    }



    private void FloraPulse(EntityUid uid, TransformComponent xform)

    {

        var damage = new DamageSpecifier();

        damage.DamageDict.Add("Radiation", 10);

        damage.DamageDict.Add("Caustic", 4);



        foreach (var ent in _lookup.GetEntitiesInRange(uid, 30f, LookupFlags.Dynamic))

        {

            if (ent == uid || !HasComp<DamageableComponent>(ent))

                continue;



            _damageable.TryChangeDamage(ent, damage);

        }



        _chat.DispatchGlobalAnnouncement(Loc.GetString("space-anomaly-flora-spore"), colorOverride: Color.LimeGreen);

    }



    private bool IsOpenSpace(Vector2 coords, MapId mapId)

    {

        var mapCoords = new MapCoordinates(coords, mapId);

        return _lookup.GetEntitiesInRange<MapGridComponent>(mapCoords, 160f).Count == 0;

    }

}


