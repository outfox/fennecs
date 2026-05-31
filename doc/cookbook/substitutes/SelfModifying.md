---
title: Self-Modifying Systems
outline: [2, 3]
---

# Lambda Calculus... in an ECS? :neofox_shocked: 

Imagine a System (something that runs code on a query) that is also in itself a State Machine and can modify its own code (not just by branching, but by referencing functors stored somewhere).

Now imagine these modifications are dependent on the data stream that goes through this system (i.e. the entities that are processed by the system).

... and NOW imagine we store these functors as Runner Delegates into Component data. 

:neofox_evil: What are the wonderful horrors of algorithmics we could summon and model here?!

# WIP - More soon, come back later! 

