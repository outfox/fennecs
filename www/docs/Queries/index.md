---
title: Queries
layout: doc
outline: [2, 3]
order: 5
---

# Queries

As a key concept behind **fenn**ecs, Queries have three main purposes.

[[toc]]

Each Query is a view into a World, representing a subset of its Entities. Queries are incredibly fast and update accordingly whenever entities spawn or their component structure changes. 

::: details (expand to see world)
A World contains Entities and their Components, as well as their structure and Relations.
![World Example: blue circle labeled world filled with fox emojis with many different traits](https://fennecs.tech/img/diagram-world.png)
:::

![Query Visualization: fox emojis with various traits grouped by common traits in several colored boxes](https://fennecs.tech/img/diagram-queries.png)




## 1. Matching & Filtering Entities
It remains associated with this specific World, and Queries can not bridge multiple Worlds. Queries use [Match Expressions](Matching.md) to define the subset of Entities they ==contain== ("match").

## 2. Processing Data (through [Stream Views](../Streams/))

The most powerful feature of Queries is that they can provide a Stream View (also known as a `zip view`) that can run code on all Entities in the Query; and even provide mutable references to component data (the Stream Types) to the delegate code being passed in.


## 3. Bulk ~~Create~~, Read, Update, Delete
Queries expose methods to operate quickly and with clear intent on all the entities matched by the query - [read more!](CRUD.md)

Use the method `Query.Despawn()` to despawn all Entities in that Query.
Alternatively, use `Query.Truncate(int, TruncateMode)` to cut your Query down to a specific size.


