---
layout: doc
title: Documentation

head:
  - - meta
    - name: description
      content: API documentation for fennecs, the tiny, tiny, high-energy Entity-Component System

---
# Welcome to the last ECS of your life!
!["using fennecs;"](https://fennecs.tech/img/using-fennecs.svg)

::: info :neofox_knives: THE COOKBOOK - Tutorials and Tricks
Feeling snackish? Try **fenn**ecs for yourself as you flip through the courses of our **[Cookbook](/cookbook/index)**. Snack on easily digestible code samples, and come back whenever you need to to jog your memory.

🍴👇👇 *Here! Grab a fork and have an amüse-gueueu... horse-œuf... treat!* 
:::

::: code-group
```cs [🦊 1, 2, 3 - gravity!]
var stream = world.Query<Vector3>().Stream();

stream.For(
    uniform: Time.Delta, 
    action: static (float dt, ref Vector3 velocity) => 
    {
        velocity.Y -= 9.81f * dt;
    }
);
```

```cs [(tighter in OTBS)]
var stream = world.Query<Vector3>().Stream();

stream.For(Time.Delta, static (float dt, ref Vector3 velocity) => {
      velocity.Y -= 9.81f * dt;
});
```
:::

::: tip :neofox_book: THE DOCS - all the Deets and Don'ts
The **Documentation** section (you're in it now!) describes the core principles of **fenn**ecs C# API. 

### Great first pick?
- [Concepts of ECS & **fenn**ecs](Concepts.md)

The nuget package also has extensive XMLdoc coverage to keep you informed while you code and explore in your IDE of choice. (neovim obviously, but also Rider or VSCode, any IDE, really)

:::


::: info :neofox_vr: THE EXAMPLES - Smol, Shiny Demos
The **[Demos](/examples/index)** category has concrete examples for a growing list of renderers and game engines! Something useful and something pretty to look at at the same time? It cannot be possible, *and yet!*
:::


::: info :neofox_pat_floof: THE REST
The **Misc** section contains our [Glossary](/misc/Glossary.md) of terms and a few [heartfelt words of thanks.](/misc/Acknowledgements.md), and anything else that didn't fit elsewhere.
:::
