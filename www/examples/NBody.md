---
title: "Demo: N-Body Problem"
layout: doc

---

# Demo: N-Body Problem
 
This demo demonstrates N:N relationships in an ECS, and how to set up and simulate an arbitrary number of bodies in a generic way.

#### Source Code
This demo is available for the following: 
| MonoGame |[Godot](https://github.com/thygrrr/fennecs/tree/main/examples/godot) | Flax | Unigine | Stride | Raylib | NeoAxis |
|:--------------:|:------------------:|:--------------:|:--------------:|:--------------:|:--------------:|:--------------:|
|![MonoGame](https://fennecs.tech/img/logo-monogame-80.png){.tile64 .nope} | ![Godot](https://fennecs.tech/img/logo-godot-80.png){.tile64} | ![Flax Engine](https://fennecs.tech/img/logo-flax-80.png){.tile64 .nope} | ![UNIGINE](https://fennecs.tech/img/logo-unigine-80-darkmode.png){.dark-only .tile64 .nope} ![UNIGINE](https://fennecs.tech/img/logo-unigine-80-lightmode.png){.light-only .tile64 .nope} | ![Stride](https://fennecs.tech/img/logo-stride-80.png){.tile64 .nope} |  ![Raylib-cs](https://fennecs.tech/img/logo-raylib-80.png){.tile64 .nope} | ![NeoAxis Engine](https://fennecs.tech/img/logo-neoaxis-80-darkmode.png){.dark-only .tile64 .nope} ![NeoAxis Engine](https://fennecs.tech/img/logo-neoaxis-80-lightmode.png){.light-only .tile64 .nope} | 

 
### Video (Godot Version)
<video controls autoplay muted loop>
<source src="https://fennecs.tech/video/fennecs-godot-nbody.mp4" type="video/mp4"/>
Your browser does not support the video tag.
</video>

State and Structure are stored in the Components on the Entities:

- 1x `Body` as Plain Component (contains mass and last position)
- Nx `Body` as Entity-Entity Relation (once for each other body)
- 1x `Position, Velocity, Acceleration` (for physics sim)

The state is transferred into the Game Engine directly talking to the Nodes.
