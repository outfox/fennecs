---
layout: doc
title: "Demo: N-Body Problem"

---

## Demo: N-Body Problem

This demo demonstrates N:N relationships in an ECS, and how to set up and simulate an arbitrary number of bodies in a generic way.

### Source Code

This demo is available for the following:

| MonoGame |[Godot](https://github.com/outfox/fennecs/tree/main/demos/godot) | Flax | Unigine | Stride | Raylib | NeoAxis |
|:--------------:|:------------------:|:--------------:|:--------------:|:--------------:|:--------------:|:--------------:|
|![MonoGame](/img/logo-monogame-80.png){.tile64 .nope} | ![Godot](/img/logo-godot-80.png){.tile64} | ![Flax Engine](/img/logo-flax-80.png){.tile64 .nope} | ![UNIGINE](/img/logo-unigine-80-darkmode.png){.dark-only .tile64 .nope} ![UNIGINE](/img/logo-unigine-80-lightmode.png){.light-only .tile64 .nope} | ![Stride](/img/logo-stride-80.png){.tile64 .nope} |  ![Raylib-cs](/img/logo-raylib-80.png){.tile64 .nope} | ![NeoAxis Engine](/img/logo-neoaxis-80-darkmode.png){.dark-only .tile64 .nope} ![NeoAxis Engine](/img/logo-neoaxis-80-lightmode.png){.light-only .tile64 .nope} |

### Video (Godot Version)

<video controls autoplay muted loop>
<source src="/video/fennecs-godot-nbody.mp4" type="video/mp4"/>
Your browser does not support the video tag.
</video>

### Remarks

Case in point here is not raw performance (although the basic GDscript-driven line trail rendering easily eats 90% of the Godot rendering time). Instead, note how short and concise the simulation loop stays - without as much as a single reverse lookup.

::: code-group

<<< ../../src/demos/godot/N-Body/NBodyDemo.cs#Showcase [Showcase]

:::

State and Structure are stored in the Components on the Entities:

- 1x `Body` as Plain Component (contains mass and last position)
- Nx `Body` as Entity-Entity Relation (once for each other body)
- 1x `Position, Velocity, Acceleration` (for physics sim)

The state is transferred into the Game Engine directly talking to the Nodes.
