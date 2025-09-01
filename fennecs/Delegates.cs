namespace fennecs;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public enum CacheHint
{
    /// <summary>
    /// Linear access during component data iteration, in unrolled loops of 8.
    /// </summary>
    /// <remarks>
    /// Trust the compiler and hardware to optimize access.
    /// </remarks>
    Monotonic,
    /// <summary>
    /// Component access is interleaved in a 4:4 block split: 0-4-1-5-2-6-3-7
    /// </summary>
    /// <remarks>
    /// An old-school technique on some architectures to improve cache efficiency when accessing multiple components.
    /// </remarks>
    Interleaved,
    /// <summary>
    /// Uses the <see cref="System.Runtime.Intrinsics.X86.Sse.Prefetch0">X86.Sse.Prefetch0</see> intrinsic (where available) to prefetch data into CPU L2/L3 cache.
    /// </summary>
    /// <remarks>
    /// Only allowed when all Stream Types are unmanaged.
    /// </remarks>
    /// <example>
    /// It may be worth trying this when dealing with 100k to a Million Matrix4x4 components in a Stream, or equivalent amounts of data.
    /// L2 caches are around 256KiB, L3 caches are in the order of 32MiB for x86/amd64 CPUs.
    /// Additionally useful when iterating over the same archetype(s) repeatedly in proximity. (e.g. running multiple delegates that access the same components)
    /// </example>
    Prefetch0,
    /// <summary>
    /// Uses the <see cref="System.Runtime.Intrinsics.X86.Sse.PrefetchNonTemporal">X86.Sse.PrefetchNonTemporal</see> intrinsic (where available) to prefetch data into CPU cache while avoiding excessive eviction of other working sets..
    /// </summary>
    /// <remarks>
    /// Only allowed when all Stream Types are unmanaged.
    /// L2 caches are around 256KiB, L3 caches are in the order of 32MiB for x86/amd64 CPUs.
    /// </remarks>
    /// <example>
    /// This may become useful when processing huge archetypes that do not fit in L3 cache (larger than 32MiB, or even 96MiB+)
    /// Greater than 8 million Matrix4x4 components, or equivalent amounts of data.
    /// L3 caches are in the order of 32MiB for x86/amd64 CPUs.
    /// </example>
    PrefetchNonTemporal,
}

public delegate bool ComponentFilter<C>(in C c0);

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
