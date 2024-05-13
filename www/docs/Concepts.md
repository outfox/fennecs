---
title: Concepts

head:
  - - meta
    - name: description
      content: Conceptual overview over the fennecs Entity-Component System
---

# Conceptual Overview

In **fenn**ecs, you spawn and despawn Entities from a World, and attach Components to them.

::: details BEHIND THE SCENES
Entites and their data are grouped into Archetypes, contiguous storages in Memory that contain all Entities with a certain costellation of Components on them. Adding or Removing components always moves an Entity from one Archetype to another.
:::


You create Queries that match certain subsets of all Entities.

You process this component data in Query runners, as well as attach or unattach more components.

Specific components can be used to further group Entities, called Relations and Object Links.
