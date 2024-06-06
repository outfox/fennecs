---
layout: doc
title: "Demo: Lots of Cubes"
---

# Demo: Lots of Cubes
 
This demo demonstrates a simple case how to update the state of a large number of Entities, and how to bring this data into a Game Engine.

#### Source Code
This demo is available for the following: 
| MonoGame |[Godot](https://github.com/outfox/fennecs/tree/main/demos/godot) | Flax | Unigine | [Stride](https://github.com/outfox/fennecs/tree/main/examples/stride) | Raylib | NeoAxis |
|:--------------:|:------------------:|:--------------:|:--------------:|:--------------:|:--------------:|:--------------:|
|![MonoGame](https://fennecs.tech/img/logo-monogame-80.png){.tile64 .nope} | ![Godot](https://fennecs.tech/img/logo-godot-80.png){.tile64} | ![Flax Engine](https://fennecs.tech/img/logo-flax-80.png){.tile64 .nope} | ![UNIGINE](https://fennecs.tech/img/logo-unigine-80-darkmode.png){.dark-only .tile64 .nope} ![UNIGINE](https://fennecs.tech/img/logo-unigine-80-lightmode.png){.light-only .tile64 .nope} | ![Stride](https://fennecs.tech/img/logo-stride-80.png){.tile64} |  ![Raylib-cs](https://fennecs.tech/img/logo-raylib-80.png){.tile64 .nope} | ![NeoAxis Engine](https://fennecs.tech/img/logo-neoaxis-80-darkmode.png){.dark-only .tile64 .nope} ![NeoAxis Engine](https://fennecs.tech/img/logo-neoaxis-80-lightmode.png){.light-only .tile64 .nope} | 

 
### Video (Godot Version)
<video controls autoplay muted loop>
<source src="https://fennecs.tech/video/fennecs-godot-cubes.mp4" type="video/mp4"/>
Your browser does not support the video tag.
</video>

::: info
All motion is 100% CPU simulation (no GPU). The point is not that this _could be done faster_ on the GPU, the point is that it _can be done fast_ on the CPU. 🦊
:::

State is stored in Components on the Entities:

- 1x `Vector3` (as Position)
- 1x `Matrix` (as Transform)
- 1x `int` (as a simple identifier)

The state is transferred into the Game Engine in bulk each frame using Stream.Raw in order to submit just the `Matrix` structs directly to the Engine's data structures for this task.

This static data is then used by the Engine's Renderer and to display the Entities.

### Video (Stride Version)
<video controls autoplay muted loop>
<source src="https://fennecs.tech/video/fennecs-stride-cubes.mp4" type="video/mp4"/>
Your browser does not support the video tag.
</video>
