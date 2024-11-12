using fennecs.storage;

// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace fennecs;


#region For/Job: Component Actions

public delegate void ComponentActionWWW<C0, C1, C2>(RW<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionWWR<C0, C1, C2>(RW<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionWRW<C0, C1, C2>(RW<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionWRR<C0, C1, C2>(RW<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionRWW<C0, C1, C2>(R<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionRWR<C0, C1, C2>(R<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionRRW<C0, C1, C2>(R<C0> comp0, R<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void ComponentActionRRR<C0, C1, C2>(R<C0> comp0, R<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;

#endregion


#region For/Job: Entity Component Actions

public delegate void EntityComponentActionWWW<C0, C1, C2>(EntityRef entity, RW<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionWWR<C0, C1, C2>(EntityRef entity, RW<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionWRW<C0, C1, C2>(EntityRef entity, RW<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionWRR<C0, C1, C2>(EntityRef entity, RW<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionRWW<C0, C1, C2>(EntityRef entity, R<C0> comp0, RW<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionRWR<C0, C1, C2>(EntityRef entity, R<C0> comp0, RW<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionRRW<C0, C1, C2>(EntityRef entity, R<C0> comp0, R<C1> comp1, RW<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void EntityComponentActionRRR<C0, C1, C2>(EntityRef entity, R<C0> comp0, R<C1> comp1, R<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;

#endregion


#region Raw: Memory Actions

public delegate void MemoryActionWWW<C0, C1, C2>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionWWR<C0, C1, C2>(Memory<C0> comp0, Memory<C1> comp1, ReadOnlyMemory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionWRW<C0, C1, C2>(Memory<C0> comp0, ReadOnlyMemory<C1> comp1, Memory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionWRR<C0, C1, C2>(Memory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionRWW<C0, C1, C2>(ReadOnlyMemory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionRWR<C0, C1, C2>(ReadOnlyMemory<C0> comp0, Memory<C1> comp1, ReadOnlyMemory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionRRW<C0, C1, C2>(ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, Memory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;
public delegate void MemoryActionRRR<C0, C1, C2>(ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2) where C0 : notnull where C1 : notnull where C2 : notnull;

#endregion


#region Raw: Entity Memory Actions

public delegate void EntityMemoryActionWWW<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionWWR<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionWRW<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionWRR<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionRWW<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionRWR<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionRRW<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

public delegate void EntityMemoryActionRRR<C0, C1, C2>(ReadOnlyMemory<Entity> entities, ReadOnlyMemory<C0> comp0, ReadOnlyMemory<C1> comp1, ReadOnlyMemory<C2> comp2)
    where C0 : notnull where C1 : notnull where C2 : notnull;

#endregion
