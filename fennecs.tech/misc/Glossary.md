---
title: Glossary
---

# Glossary of Terms
These are terms that are often casually used, explaine a little more in depth.

## Structural Changes

Changes to the layout of Entities - which Components, Links, or Relations it has -  will cause data to be moved around internally, and sometimes internal data structures need to resize. This may cause individual Entities or even entire memory regions to be moved within their affected Archetype.

#### Each of these constitutes a structural change:
 - Adding a Component, Link, or Relation
 - Removing a Component, Link, or Relation
 - Despawning an Entity

#### The special case of Spawning

- Spawning instantly returns a fully usable Entity builder struct... however, its Identity will only be written to the World later, so it is invisible to Queries and even to `World.GetEntity` and `World.IsAlive`.

Structural changes to the world are deferred while a [World Lock](World%Lock.md) is taken out, until _ALL_ locks are disposed. Once that happens, they are all applied in order of submission.
