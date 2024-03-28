---
title: Queries
layout: doc
---

# Queries

A Query is a view into a World, representing a subset of its Entities. It remains associated with this specific World, and Queries can not bridge multiple Worlds.

They serve the following primary purposes:

[[toc]]


### 1. Filtering & Tracking Archetypes
Queries use [Filter Expressions](FilterExpressions.md) to define the subset of Entities they ==contain== ("match").


### 2. CRUD - Create, Read, Update, Delete
Queries expose methods to operate quickly and with clear intent on all the entities matched by the query - [read more!](CRUD.md)

Use the method `Query.Despawn()` to despawn all Entities in that Query.
Alternatively, use `Query.Truncate(int, TruncateMode)` to cut your Query down to a specific size.


### 3. Processing (via [Stream Queries](Query.1-5.md))







