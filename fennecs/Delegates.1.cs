﻿using fennecs.storage;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace fennecs;


#region For/Job: Component Actions

public delegate void ComponentActionW<C0>(RW<C0> comp0) where C0 : notnull;
public delegate void ComponentActionR<C0>(R<C0> comp0) where C0 : notnull;

#endregion


#region For/Job: Entity Component Actions

public delegate void EntityComponentActionW<C0>(EntityRef entity, RW<C0> comp0) where C0 : notnull;
public delegate void EntityComponentActionR<C0>(EntityRef entity, R<C0> comp0) where C0 : notnull;

#endregion
