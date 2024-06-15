---
title: 6. One Ring (Links)
outline: [2, 3]
order: 6
---

#  One Ring to Find Them

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs principles.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/outfox/fennecs/blob/main/cookbook) 

:::
### Premise
In this example, we'll recreate the forging of the Rings of Power in the Land of Mordor, as told in the legendary story. 

We'll use fennecs' Object Link system to model the binding relationship between the One Ring and the other Rings it rules.

First, we spawn Entities for each of the other Rings and its Bearer, and link all of them to our singleton instance of the One Ring.

Then, we bind them all in darkness with a `Stream<>.For` runner.

### Recipe
::: code-group
<<< ../../../cookbook/OneRing.cs {cs:line-numbers} [Implementation]
<<< ../../../cookbook/OneRing.output.txt{txt} [Output]
:::

In this example:

1. We define the `RingBearer` and `OneRing` components to represent the bearers of the Rings and the power of the One Ring respectively.
2. We refer to a singleton instnace of `OneRing` to represent the One Ring.
3. We create Entities for each group of Rings (Elven, Dwarven, and Human) using the `EntitySpawner`, adding a `RingBearer` component and a Link to the One Ring for each.
4. We create a Query to find all `RingBearer` Entities that are linked to our `OneRing` instance (the One Ring).
5. We use the Query's `For` method to iterate over the linked Rings and corrupt their bearers, updating the `corrupted` flag in the `RingBearer` record.

This example showcases the expressive power of fennecs' Object Link system. By establishing Links from the Rings to the One Ring, we are able to easily query and manipulate all Entities bound to it. The One Ring's influence is modeled directly in the ECS architecture.

The use of records for the `RingBearer` component allows us to cleanly update the corruption status of each bearer in a single line of code within the `For` loop.

#### One ECS to rule them all, one Query to find them, 
#### One .NET package to bring them all and the darkness bind them.  
#### In the Land of Open Source where the Foxes fly.
