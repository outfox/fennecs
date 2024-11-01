namespace fennecs;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public delegate void EntityAction(Entity entity);
public delegate void EntitySpanAction(Span<Entity> entities);

public delegate void UniformEntityAction<in U>(U uniform, Entity entity);
public delegate void UniformEntitySpanAction<in U>(U uniform, Span<Entity> entities);

public delegate void CovariantAction<in C0>(C0 comp0);
public delegate void ComponentAction<C0>(ref C0 comp0);
public delegate void ComponentAction<C0, C1>(ref C0 comp0, ref C1 comp1);
public delegate void ComponentAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void ComponentAction<C0, C1, C2, C3>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
public delegate void ComponentAction<C0, C1, C2, C3, C4>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);

public delegate void UniformComponentAction<in U, C0>(U uniform, ref C0 comp0);
public delegate void UniformComponentAction<in U, C0, C1>(U uniform, ref C0 comp0, ref C1 comp1);  
public delegate void UniformComponentAction<in U, C0, C1, C2>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void UniformComponentAction<in U, C0, C1, C2, C3>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
public delegate void UniformComponentAction<in U, C0, C1, C2, C3, C4>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);

public delegate void EntityComponentAction<C0>(in Entity entity, ref C0 comp0);
public delegate void EntityComponentAction<C0, C1>(in Entity entity, ref C0 comp0, ref C1 comp1);
public delegate void EntityComponentAction<C0, C1, C2>(in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void EntityComponentAction<C0, C1, C2, C3>(in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3); 
public delegate void EntityComponentAction<C0, C1, C2, C3, C4>(in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);

public delegate void UniformEntityComponentAction<in U, C0>(U uniform, in Entity entity, ref C0 comp0);
public delegate void UniformEntityComponentAction<in U, C0, C1>(U uniform, in Entity entity, ref C0 comp0, ref C1 comp1);
public delegate void UniformEntityComponentAction<in U, C0, C1, C2>(U uniform, in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void UniformEntityComponentAction<in U, C0, C1, C2, C3>(U uniform, in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
public delegate void UniformEntityComponentAction<in U, C0, C1, C2, C3, C4>(U uniform, in Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);

public delegate void EntityUniformComponentAction<in U, C0>(in Entity entity, U uniform, ref C0 comp0);
public delegate void EntityUniformEntityComponentAction<in U, C0, C1>(in Entity entity, U uniform, ref C0 comp0, ref C1 comp1);
public delegate void EntityUniformEntityComponentAction<in U, C0, C1, C2>(in Entity entity, U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void EntityUniformEntityComponentAction<in U, C0, C1, C2, C3>(in Entity entity, U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
public delegate void EntityUniformEntityComponentAction<in U, C0, C1, C2, C3, C4>(in Entity entity, U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);

public delegate void MemoryAction<C0>(Memory<C0> comp0);
public delegate void MemoryAction<C0, C1>(Memory<C0> comp0, Memory<C1> comp1);  
public delegate void MemoryAction<C0, C1, C2>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2);
public delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3);
public delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3, Memory<C4> comp4);

public delegate void MemoryUniformAction<in U, C0>(U uniform, Memory<C0> comp0);
public delegate void MemoryUniformAction<in U, C0, C1>(U uniform, Memory<C0> comp0, Memory<C1> comp1);
public delegate void MemoryUniformAction<in U, C0, C1, C2>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2); 
public delegate void MemoryUniformAction<in U, C0, C1, C2, C3>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3);
public delegate void MemoryUniformAction<in U, C0, C1, C2, C3, C4>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3, Memory<C4> comp4);

public delegate void MemoryUniformEntityAction<in U, C0>(U uniform, ReadOnlyMemory<Entity> entities, Memory<C0> comp0);
public delegate void MemoryUniformEntityAction<in U, C0, C1>(U uniform, ReadOnlyMemory<Entity> entities, Memory<C0> comp0, Memory<C1> comp1);
public delegate void MemoryUniformEntityAction<in U, C0, C1, C2>(U uniform, ReadOnlyMemory<Entity> entities, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2); 
public delegate void MemoryUniformEntityAction<in U, C0, C1, C2, C3>(U uniform, ReadOnlyMemory<Entity> entities, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3);
public delegate void MemoryUniformEntityAction<in U, C0, C1, C2, C3, C4>(U uniform, ReadOnlyMemory<Entity> entities, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3, Memory<C4> comp4);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
