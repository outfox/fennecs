---
title: 2. Kill Bill (Relations)
outline: [2, 3]
---

# Paying a Visit to Old Friends

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs features.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/examples/cookbook) 

:::

### Premise
To settle an old score, we need to get even with five ~~former friends~~ Entities...

We create the Entities and define the [Relation](../docs/Relation.md) (`struct Betrayed`) they have with us, and also ours (`struct Grudge`) with them. We include a plain [Component](../docs/Component.md) (`struct Location`) as useful data to everyone involved.

Next, we query for the Relation, say hello, and ~~unalive~~ interact with the Entities in a [Query.For](../docs/Queries/Query.For.md).

### Implementation
<<< ../../cookbook/KillBill.cs {cs:line-numbers}

### Outcome
The above code is the actual code, yet this output is copy-pasta and may not be re-generated each time the code is updated. *(mumble CI-CD mumble TODO mumble)*
```shell
dotnet run --project KillBill.csproj
```
```txt 
As we said, there were 5 of them.
One hides in hideout 0x140d9825.
One hides in hideout 0x2328bb6c.
One hides in hideout 0x3573a7b3.
One hides in hideout 0x1414c263.
One hides in hideout 0x4acbcdcf.
We are still here.
Do we hold grudges? True.
Oh, hello E-00000002:00001! Remember E-00000001:00001?
Oh, hello E-00000003:00001! Remember E-00000001:00001?
Oh, hello E-00000004:00001! Remember E-00000001:00001?
Oh, hello E-00000005:00001! Remember E-00000001:00001?
Oh, hello E-00000006:00001! Remember E-00000001:00001?
Now, there are 0 of them.
Let's get out of hideout 0x4acbcdcf.
We've been traveling for a while.
Any more grudges? False.
```