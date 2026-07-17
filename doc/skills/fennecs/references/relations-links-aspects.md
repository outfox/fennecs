# Relations, Object Links & Aspects — full reference (fennecs 0.7.0)

## Entity-Entity Relations

A Relation is an ordinary component whose storage key also carries a target
`Entity`. Any type can back a relation; the data is per-(type, target) pair,
and one entity can hold the same relation type toward many targets.

```csharp
record struct Owes(decimal Amount);

bob.Add(new Owes(10M), alice);      // Owes -> alice
bob.Add(new Owes(23M), eve);        // Owes -> eve (coexists)
bob.Add<Betrayed>(us);              // newable backing type, no data needed

bob.Has<Owes>(alice);               // specific target
bob.Has<Owes>(Entity.Any);          // any target
ref var debt = ref bob.Ref<Owes>(eve);
bob.Remove<Owes>(alice);
```

Key facts:

- **Unidirectional.** The target doesn't know it's targeted. For backlinks,
  store the target inside the backing data too:
  `record struct Child(Entity Target); me.Add(new Child(parent), parent);`
- **Target must be alive** when the relation is created.
- **Despawn cleanup:** despawning a target removes all relations pointing at
  it from every other entity, automatically.
- **Same world only:** a relation may not target an entity from another World
  — every creation path throws on that.
- **Grouping is the point:** relations participate in archetype identity, so
  entities relating to the same target are stored contiguously — but `n`
  distinct targets can mean up to `n!` archetype permutations. Use relations
  to group *families* of entities, not as a general-purpose pointer (a plain
  `Entity` field inside a component is fine for that and causes no
  fragmentation).

### Querying relations

```csharp
world.Query<Owes>(eve).Compile();                        // to specific target
world.Query<Owes>(Entity.Any).Compile();                 // to any target
world.Query<Owes>(Entity.Any).Not<Owes>(eve).Compile();  // any except eve
world.Query().Not<Owes>(Entity.Any).Compile();           // debt-free entities
```

Remember: `world.Query<Owes>()` (default = `Match.Plain`) matches only *plain*
`Owes` components — **not** relations. This is the #1 gotcha.

With a wildcard stream type, iteration yields the entity once per matching
relation (cross-join) — that's how you process each (entity, target) pair.
The 3-Body/N-Body pattern relies on this to accumulate forces from every
attractor in a single `For`.

## Object Links

A Link makes a reference-type *object* the secondary key — and the object is
also the component's data. Entities linked to the same object are grouped in
the same archetype and get the live object during iteration.

```csharp
PhysicsWorld physics = new();                    // any class instance

entity.Add(Link.With(physics));                  // link (Type PhysicsWorld -> physics)
entity.Has<PhysicsWorld>(Match.Object);          // any link of that type
entity.Has(Link.With(physics));                  // that specific link
entity.Remove(physics);                          // type-inferred remove

var stream = world.Query<Position, PhysicsWorld>(Match.Plain, Link.With(physics)).Stream();
stream.For((ref Position pos, ref PhysicsWorld pw) => pw.Sync(pos));
```

Typical uses: partition entities by level/chunk/simulation island; hand every
entity access to a shared engine object (scene, RNG, physics) without a
singleton; multiple links of the same type per entity are allowed.

`Link.Any` ≡ `Match.Object`. The same fragmentation caution as relations
applies. For a single shared blob without archetype grouping, a plain
reference-type component (a "Shareable" — same instance added to many
entities) is the lighter tool.

## Aspects (new in 0.7.0)

An Aspect is a self-contained set of archetypes inside a World — its own
contiguous component-storage universe. All Aspects share the same Entities;
only component data lives apart. Purpose: **fight fragmentation** by giving
hot component types (e.g. `Position`, `Matrix` for rendering) a storage
universe that gameplay component churn can't splinter.

Every World has a built-in Aspect `World.Main` (`IsMain == true`); any type
not explicitly owned elsewhere is stored there — which is why single-aspect
code never notices Aspects exist.

```csharp
class GameWorld : World
{
    public readonly Aspect Visuals;
    public GameWorld()
    {
        // register before any entity materializes the type (ownership freezes
        // on first use)
        Visuals = AddAspect("visuals").Owns<Position>().Owns<Matrix4x4>();
    }
}

using var world = new GameWorld();
var e = world.Spawn()
    .Add(new Position(1, 2))   // routed to "visuals" automatically
    .Add(new CrewData(3));     // routed to Main

// Query the hot universe: contiguous, cache-friendly
world.Visuals.Stream<Position>().For((ref Position p) => …);
```

Rules and API:

| Member | Notes |
|---|---|
| `world.AddAspect(name)` / `AddAspect(name, params Type[])` | unique name or `ArgumentException` |
| `aspect.Owns<T>()` / `.Owns(params Type[])` | declare owned types; frozen once the type is first stored |
| `world.StrictAspects { init; }` | opt-in: unregistered types throw instead of routing to Main |
| `world.Aspects`, `world.Main` | Main is always first |
| `aspect.Query<…>()`, `.Stream<…>()`, `.All`, `.Count`, `IEnumerable<Entity>` | full query surface, scoped to the aspect |
| `IAspect` | common interface of `World` and `Aspect` — write systems against `IAspect` and they run on either |

- `world.Query<T>()` resolves automatically to the Aspect that owns `T` — you
  rarely need to name the aspect.
- **A Query (and Batch) cannot span Aspects** — mixing types owned by
  different aspects in one query throws. For occasional cross-aspect access,
  use `entity.Ref<T>()` per component; for systematic co-iteration, the types
  belong in the same aspect.
- **Don't prematurely foximize:** one Main aspect is right for most games.
  Reach for Aspects when profiling shows archetype fragmentation (many small
  archetypes in hot loops; check `world.DebugString()` for counts/sizes) —
  typically at hundreds of thousands of entities.
