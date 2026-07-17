---
layout: doc
title: Documentation
prev: false
description: 'API documentation for fennecs, the tiny, high-energy Entity-Component System for C# - covering Worlds, Entities, Queries, Streams, and Aspects.'
---
!["using fennecs;"](/img/using-fennecs-darkmode.svg){.dark-only}
!["using fennecs;"](/img/using-fennecs-lightmode.svg){.light-only}

::: info :neofox_knives: THE COOKBOOK - Tutorials and Tricks
Can't wait? Try **fenn**ecs for yourself as you flip through the courses of our **[Cookbook](/cookbook/index)**. 
:::

::: code-group
```cs [🦊 1, 2, 3 - gravity!]
var stream = world.Stream<Vector3>();

stream.For(Time.Delta, (dt, ref velocity) => 
{
    velocity.Y -= 9.81f * dt;
});
```

```cs [(🧓 same, in classic C#)]
var stream = world.Stream<Vector3>();

stream.For(
    uniform: Time.Delta, 
    action: (float dt, ref Vector3 velocity) => 
    {
        velocity.Y -= 9.81f * dt;
    }
);
```

:::

::: tip :neofox_glasses: BEST SERVED WITH C# 14
**fenn**ecs is at its best with **C# 14 or later** *(the default language version for .NET 10)*. Since C# 14, lambda parameters may omit their types even when they carry modifiers like `ref` and `in`  –  so runner delegates shrink from `(float dt, ref Vector3 velocity)` to `(dt, ref velocity)`. The docs use this short form throughout.

**The difference on older C#:** before C# 14, any lambda parameter with a `ref`/`in`/`out` modifier had to spell out its type (and then *all* parameters needed explicit types). Everything in these docs still works on C# 12/13  –  just write the parameter types back in, exactly as shown in the "classic" tab above.
:::

::: tip :neofox_book: THE DOCUMENTATION - (you're in it now!)
All the deets & don'ts and core facets of the **fenn**ecs API. Use the navigation menu on the left!

### Where to begin?
#### 👉 [**Concepts**](Concepts.md) - an overview of **fenn**ecs (and ECS in general)
#### 👉 [**Advanced**](Advanced/index.md) - sharp tools for big simulations *(new: [Aspects](Advanced/Aspects/index.md))*

The nuget package also has extensive XMLdoc coverage to keep you informed while you code and explore in your IDE of choice. (neovim obviously, but also Rider or VSCode, any IDE, really)

:::


::: info :neofox_vr: THE EXAMPLES - Smol, Shiny Demos
The **[Demos](/examples/index)** category has concrete examples for a growing list of renderers and game engines! Something useful and something pretty to look at at the same time? It cannot be possible, *and yet!*
:::


::: info :neofox_pat_floof: THE REST
The **Misc** section contains our [Glossary](/misc/Glossary.md) of terms and a few [heartfelt words of thanks.](/misc/Acknowledgements.md), and anything else that didn't fit elsewhere. Like our [Roadmap](/misc/Roadmap.md) and [Changelog](/misc/Changelog.md)!
:::


