---
layout: doc
title: Documentation

head:
  - - meta
    - name: description
      content: API documentation for fennecs, the tiny, tiny, high-energy Entity-Component System

---
!["using fennecs;"](https://fennecs.tech/img/using-fennecs-darkmode.svg){.dark-only}
!["using fennecs;"](https://fennecs.tech/img/using-fennecs-lightmode.svg){.light-only}

::: info :neofox_knives: THE COOKBOOK - Tutorials and Tricks
Can't wait? Try **fenn**ecs for yourself as you flip through the courses of our **[Cookbook](/cookbook/index)**. 
:::

::: code-group
```cs [🦊 1, 2, 3 - gravity!]
var stream = world.Stream<Vector3>();

stream.For(
    uniform: Time.Delta, 
    action: (float dt, ref Vector3 velocity) => 
    {
        velocity.Y -= 9.81f * dt;
    }
);
```

```cs [(🤏 200% tighter in OTBS)]
var stream = world.Stream<Vector3>();

stream.For(Time.Delta, (float dt, ref Vector3 velocity) => {
      velocity.Y -= 9.81f * dt;
});
```

```cs [soon: (🚀 700% faster with SIMD)]
var simd = world.Query<Vector3>().SIMD();
// this is still under hot & messy development right now 😅
simd.Add(
      operand: new Comp<Vector3>(), // can be same as destination!
      uniform: Time.Delta * 9.81f * Vector3.UnitZ,
      destination: new Comp<Vector3>(),
});
```


:::

::: tip :neofox_book: THE DOCUMENTATION - (you're in it now!)
All the deets & don'ts and core facets of the **fenn**ecs API. Use the navigation menu on the left!

### Where to begin?
#### 👉 [**Concepts**](Concepts.md) - an overview of **fenn**ecs (and ECS in general)

The nuget package also has extensive XMLdoc coverage to keep you informed while you code and explore in your IDE of choice. (neovim obviously, but also Rider or VSCode, any IDE, really)

:::


::: info :neofox_vr: THE EXAMPLES - Smol, Shiny Demos
The **[Demos](/examples/index)** category has concrete examples for a growing list of renderers and game engines! Something useful and something pretty to look at at the same time? It cannot be possible, *and yet!*
:::


::: info :neofox_pat_floof: THE REST
The **Misc** section contains our [Glossary](/misc/Glossary.md) of terms and a few [heartfelt words of thanks.](/misc/Acknowledgements.md), and anything else that didn't fit elsewhere. Like our [Roadmap](/misc/Roadmap.md) and [Changelog](/misc/Changelog.md)!
:::
