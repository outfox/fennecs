---
title: 1. Star Trek (Entities)
outline: [2, 3]
---

# Famous Captains, gone - one by one!

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs principles.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/cookbook) 

:::

### Premise
Let's shed some light on a couple of the more famous Captains in the Star Trek Universe.

This example will teach a little about what happens to Entities when they Despawn!

We create a unique [Entity](../docs/Entity.md) for each; but `kirk` gets despawned and replaced by The Next Generation (`picard`). Then we create a bunch more and output to visualize how recycled Entities compare to each other and to give a feeling over how a [World](../docs/World.md) counts Generations up when Entities are destroyed.

### Implementation
<<< ../../cookbook/StarTrek.cs {cs:line-numbers}

### Outcome
The above code is the actual code, yet this output is copy-pasta and may not be re-generated each time the code is updated. *(mumble CI-CD mumble TODO mumble)*
```shell
dotnet run --project StarTrek.csproj
```
```txt 
      Kirk is E-00000001:00001 ... boldly going!
    Picard is E-00000001:00002 - the next Generation!
   Janeway is E-00000002:00001 - the best Captain ever!
      Kirk is E-00000001:00001 - and alive? False
    Archer is E-00000001:00003, ugh, don't we hate reboots...
  Georgiou is E-00000002:00002, now THAT's a Captain!
   Shatner ain't Stewart, even in death! True
```
