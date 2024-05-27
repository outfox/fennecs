---
title: SIMD Operations
---

# Blit
The most prominent SIMD operation is `Query<C>.Blit`, which writes the component value to all entities in the Query. `C` must be one of the Query's [Stream Types](StreamTypes.md).

Fast Vectorization techniques are used to `Blit` Component Types that are fully Blittable (e.g. `struct` with only primitive fields), but all Reference types are also supported. (they blit rapidly, but not as fast as Blittable types)

# WIP :neofox_what:
Work in Progress - more coming soon.