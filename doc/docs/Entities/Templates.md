---
title: Templates
menu: Templates
order: 2
outline: [1, 2]
description: 'Entity templates and fast bulk spawning in fennecs with the EntityTemplate (World.Template): reusable templates, required per-spawn components via Needs, spawning waves straight into their final Archetype, and getting spawned Entity handles back via Span.'
---

# Templates

::: tip :neofox_thumbsup: Waves, Templates & Factories
Need a hundred thousand entities? A reusable template for your werewolf packs? The `EntityTemplate` spawns entities *directly into their final Archetype* — no churning, no ceremony, all speed.
:::

The `EntityTemplate` is a reusable entity blueprint that doubles as a bulk spawner. Get one via `World.Template()`, configure it with components, then spawn as many entities as you need!

**Benefits:**
- Entities spawn directly into their final archetype (no churning!)
- Data is blitted directly to storage (lightning-fast)
- Reusable for spawning waves of similar entities

## Typical Use
```cs
var world = new World();

using var template = world.Template() // Requests an EntityTemplate
    .Add(new Velocity { X = 50 })
    .Add<Bee>()
    .Spawn(100_000); // AAAAAAAAAA!
```

::: tip :neofox_thumbsup: Dispose for Extra Credit
`EntityTemplate` implements `IDisposable` to return pooled data structures for reuse. No memory leaks either way, but disposing is a nice habit!
:::


## Repeat Spawns & Templates

Templates can be modified and reused:

```cs
using var humanTemplate = world.Template()
    .Add(new Health { Value = 100 })
    .Add<Dexterity>(12) // Stats do well with conversion operators for int!
    .Add<Charisma>(15)
    //.Add<...> (more omitted here)
    .Add<Human>();

var him = humanTemplate.Spawn(); // Spawn a single human... 
var her = humanTemplate.Spawn(); // ... and another! (Spawn() returns the Entity)

// The using statement disposes the template at end of scope/function
using var werewolfTemplate = world.Template()
    .Add(new Health { Value = 250 })
    .Add<Werewolf>()
    .Spawn(9); // 9 regulars

werewolfTemplate.Add<Elite>(); // anything it spawns from now on has Elite!
werewolfTemplate.Spawn(5); //+5 Elites, giving the BEST NUMBER of werewolves: 14!

return; // werewolfTemplate is automatically disposed here by the using statement.
```


## Required Components (`Needs`)

Some components don't belong *in* the template — they belong to each spawned entity individually. A werewolf pack shares its `Werewolf` tag, but every wolf deserves its own `Name`. Declare such components with `Needs<C>()`, and every `Spawn` **must** provide them — checked at compile time:

```cs
var pack = innistrad.Template()
    .Add<Werewolf>()      // baked: shared by the whole pack
    .Needs<Name>()        // required: EntityTemplate<Name>
    .Needs<Health>();     // required: EntityTemplate<Name, Health>

pack.Spawn(new Name("Chonker"), new Health(9000));
pack.Spawn(10, new Name("Chonker"), new Health(9000));
//TEN chonky werewolves, hahahah 🦇

pack.Spawn(new Name("Fluffy")); // ⛔ compile error: where's the Health?
```

Each `Needs<C>()` consumes the template and returns a wider one (up to 6 required components), so declare requirements in one fluent chain. Trying to `Add` a required component, `Needs` an added one, or declare the same requirement twice all throw — a template never lies about who provides what.

::: tip :neofox_thumbsup: Sugar-Free Sweetness
Give your components implicit conversion operators and the ceremony melts away — `pack.Spawn(10, "Chonker", 9000)` works the moment `Name` converts from `string` and `Health` from `int`. No reflection, no magic, just C#.
:::

### Per-Entity Values in Bulk

Uniform waves are just the start. Provide *each* entity's values via a factory or pre-filled spans:

```cs
// Factory: called once per entity with its index
pack.Spawn(100, i => (new Name($"Wolf #{i}"), new Health(250 + i)));

// Spans: zero-delegate, data-driven, handles delivered too
var dest = new Entity[names.Length];
pack.Spawn(dest, names, healths); // one entity per element, values by index
```

### Required Relations

Relations can be required too: the *target* is baked into the template, the backing *value* arrives per spawn.

```cs
using var follower = world.Template()
    .Add<Werewolf>()
    .Needs<Loyalty>(alphaWolf); // target fixed, value per-spawn

follower.Spawn(10, i => new Loyalty(1.0f - i * 0.05f)); // waning devotion
```


## Entity Returns (for further processing)

Sometimes you spawn a wave and want the handles right away — to wire up relations, hand them to game logic, or track them somewhere. Each `Spawn` overload has you covered:

| Overload | Returns | Use When |
|----------|---------|----------|
| `Spawn()` | `Entity` | You want one entity and its handle |
| `Spawn(int count)` | `EntityTemplate` (fluent) | Fire-and-forget bulk spawning |
| `Spawn(Span<Entity> destination)` | `EntityTemplate` (fluent) | You want the handles of a whole wave |

The `Span` overload spawns **one entity per element** of the span and writes their handles into it — the span's length *is* the spawn count. The handles are plain `Entity` values (not views into World storage), so they stay valid until the entities despawn. Keep them as long as you like!

```cs
// A single entity, handle returned directly:
var leader = world.Template()
    .Add<Werewolf>()
    .Spawn();

// A whole pack, delivered into your buffer (array, stackalloc, wherever):
var pack = new Entity[13];
using var template = world.Template()
    .Add<Werewolf>()
    .Spawn(pack); // fills all 13 slots, still fluent!

foreach (var wolf in pack) wolf.Add<PackMember>(leader); // relation to the leader
```

::: tip :neofox_science: Zero-Cost Delivery
The handles are minted directly into your span during the spawn — no copying, no allocation. Fire-and-forget `Spawn(count)` stays just as fast as before.
:::


## Quick Reference

| Scenario | Recommended |
|----------|-------------|
| Single entity, few components | [`World.Spawn()`](/docs/Entities/Spawning.md) |
| Single entity, many components | `EntityTemplate` |
| Bulk spawning (10+ entities) | `EntityTemplate` |
| Entity templates/factories | `EntityTemplate` |
| Prototyping/debugging | [`World.Spawn()`](/docs/Entities/Spawning.md) |

::: info :neofox_science: Performance Note
For spawning 100+ similar entities, `EntityTemplate` can be 10-100x faster than individual `World.Spawn()` calls due to direct archetype placement and memory blitting.
:::
