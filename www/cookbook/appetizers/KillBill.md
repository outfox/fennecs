---
title: 4. Kill Bill (Relations)
outline: [2, 3]
order: 4

---

# Paying a Visit to Old Friends

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs principles.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/cookbook) 

:::

### Premise
To settle an old score, we need to get even with five ~~former friends~~ Entities... we need to find those that wronged us, and wrong them right back in their face.

We create the Entities and define the [Relation](../../docs/Relation.md) (`struct Betrayed`) they have with us, and also ours (`struct Grudge`) with them. We include a plain [Component](/docs/Components/) (`struct Location`) as useful data to everyone involved.

Next, we query for the Relation, say hello, and ~~unalive~~ interact with the Entities in a [Query.For](../../docs/Queries/StreamQueries/Query.For.md). This removes our `Grudge` for them.

### Recipe
::: code-group
<<< ../../../cookbook/KillBill.cs {cs:line-numbers} [Implementation]
<<< ../../../cookbook/KillBill.output.txt{txt} [Output]
:::
