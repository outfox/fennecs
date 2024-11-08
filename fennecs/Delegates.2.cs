using fennecs.storage;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace fennecs;


#region For/Job: Component Actions

public delegate void ComponentActionWW<C0, C1>(RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionWR<C0, C1>(RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionRW<C0, C1>(R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void ComponentActionRR<C0, C1>(R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;

#endregion


#region For/Job: Entity Component Actions

public delegate void EntityComponentActionWW<C0, C1>(EntityRef entity, RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionWR<C0, C1>(EntityRef entity, RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionRW<C0, C1>(EntityRef entity, R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityComponentActionRR<C0, C1>(EntityRef entity, R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;

#endregion


#region Raw: Memory Actions

public delegate void MemoryActionWW<C0, C1>(Memory<C0> comp0, Memory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void MemoryActionWR<C0, C1>(Memory<C0> comp0, ReadOnlyMemory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void MemoryActionRW<C0, C1>(ReadOnlyMemory<C0> comp0, Memory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void MemoryActionRR<C0, C1>(ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1) where C0 : notnull where C1 : notnull;

#endregion


#region Raw: Entity Memory Actions

public delegate void EntityMemoryActionWW<C0, C1>(ReadOnlyMemory<Entity> entities, Memory<C0> comp0, Memory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityMemoryActionWR<C0, C1>(ReadOnlyMemory<Entity> entities, Memory<C0> comp0, ReadOnlyMemory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityMemoryActionRW<C0, C1>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, Memory<C1> comp1) where C0 : notnull where C1 : notnull;
public delegate void EntityMemoryActionRR<C0, C1>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1) where C0 : notnull where C1 : notnull;

#endregion
