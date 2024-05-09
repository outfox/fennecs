---
title: 1. Star Trek (Entities)
outline: [2, 3]
---

# Famous ~~Captains~~ Entities - none alike!

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs principles.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/cookbook) 

:::

### Premise
Let's reminisce some famous Captains of old and new.

This example will explore what Entities "look" like in your logs, and what happens when they Despawn, and how they get recycled.

We spawn a unique [Entity](/docs/Entities/index.md) for each captain; but `kirk` gets despawned and replaced by The Next Generation (`picard`). Then we create a bunch more and output to visualize how recycled Entities compare to each other and to give a feeling over how a [World](/docs/World.md) counts Generations up when Entities are destroyed.

### Recipe
::: code-group
<<< ../../../cookbook/StarTrek.cs {cs:line-numbers} [Implementation]
<<< ../../../cookbook/StarTrek.output.txt{txt} [Output]
:::
