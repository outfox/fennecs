---
title: 2. Kill Bill (Relations)
outline: [2, 3]
---

# Paying a Visit to Old Friends

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs features.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/examples/cookbook) 

:::

### Premise
To settle an old score, we need to get even with five ~~former friends~~ Entities...

We create the Entities and define the [Relation](../docs/Relation.md) (`struct Betrayed`) they have with us, and also ours (`struct Grudge`) with them. We include a plain [Component](../docs/Component.md) (`struct Location`) as useful data to everyone involved.

Next, we query for the Relation, say hello, and ~~unalive~~ interact with the Entities in a [Query.For](../docs/Queries/Query.For.md).

### Implementation
<<< ../../examples/cookbook/KillBill.cs

### Outcome
The above code is the actual code, yet this output is copy-pasta and may not be re-generated each time the code is updated. *(mumble mumble TODO mumble)*
```shell
dotnet run KillBill
```
```txt 
As we said, there were 5 of them.
One hides in hiding place 0x69ac29a0.
One hides in hiding place 0x772c9554.
One hides in hiding place 0x534770fe.
One hides in hiding place 0x31ecd8bc.
One hides in hiding place 0x4f38b04b.
We are still here.
Do we hold grudges? True.
Oh, hello E-00000002:00001! Remember E-00000001:00001?
Oh, hello E-00000003:00001! Remember E-00000001:00001?
Oh, hello E-00000004:00001! Remember E-00000001:00001?
Oh, hello E-00000005:00001! Remember E-00000001:00001?
Oh, hello E-00000006:00001! Remember E-00000001:00001?
Now, there are 0 of them.
Let's get out of hiding place 0x4f38b04b.
Now we are traveling.
Any more grudges? False.
```