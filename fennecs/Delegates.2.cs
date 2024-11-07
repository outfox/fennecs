using fennecs.storage;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace fennecs;

public delegate void ComponentActionWW<C0, C1>(RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionWR<C0, C1>(RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionRW<C0, C1>(R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionRR<C0, C1>(R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;


public delegate void EntityComponentActionWW<C0, C1>(EntityRef entity, R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionRR<C0, C1>(EntityRef entity, R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionWR<C0, C1>(EntityRef entity, RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionRW<C0, C1>(EntityRef entity, R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
