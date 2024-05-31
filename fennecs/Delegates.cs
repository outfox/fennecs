namespace fennecs;

/// <summary>
/// Basic delegate to notify about something concerning an Entity.
/// </summary>
public delegate void EntityAction(Entity e);

/// <summary>
/// Delegate to notify about something concerning a sequence of Entities.
/// </summary>
public delegate void EntitySpanAction(Span<Entity> e);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefAction"]'/>
public delegate void ComponentAction<C0>(ref C0 c0);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefAction"]'/>
public delegate void ComponentAction<C0, C1>(ref C0 c0, ref C1 c1);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefAction"]'/>
public delegate void ComponentAction<C0, C1, C2>(ref C0 c0, ref C1 c1, ref C2 c2);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefAction"]'/>
public delegate void ComponentAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefAction"]'/>
public delegate void ComponentAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefActionU"]'/>
public delegate void UniformComponentAction<C0, in U>(ref C0 c0, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefActionU"]'/>
public delegate void UniformComponentAction<C0, C1, in U>(U uniform, ref C0 c0, ref C1 c1);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefActionU"]'/>
public delegate void UniformComponentAction<C0, C1, C2, in U>(ref C0 c0, ref C1 c1, ref C2 c2, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefActionU"]'/>
public delegate void UniformComponentAction<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:RefActionU"]'/>
public delegate void UniformComponentAction<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityAction"]'/>
public delegate void EntityComponentAction<C0>(Entity e, ref C0 c0);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityAction"]'/>
public delegate void EntityComponentAction<C0, C1>(Entity e, ref C0 c0, ref C1 c1);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityAction"]'/>
public delegate void EntityComponentAction<C0, C1, C2>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityAction"]'/>
public delegate void EntityComponentAction<C0, C1, C2, C3>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityAction"]'/>
public delegate void EntityComponentAction<C0, C1, C2, C3, C4>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityActionU"]'/>
public delegate void UniformEntityComponentAction<C0, in U>(Entity e, ref C0 c0, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityActionU"]'/>
public delegate void UniformEntityComponentAction<C0, C1, in U>(Entity e, ref C0 c0, ref C1 c1, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityActionU"]'/>
public delegate void UniformEntityComponentAction<C0, C1, C2, in U>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityActionU"]'/>
public delegate void UniformEntityComponentAction<C0, C1, C2, C3, in U>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:EntityActionU"]'/>
public delegate void UniformEntityComponentAction<C0, C1, C2, C3, C4, in U>(Entity e, ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryAction"]'/>
public delegate void MemoryAction<C0>(Memory<C0> c0);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryAction"]'/>
public delegate void MemoryAction<C0, C1>(Memory<C0> c0, Memory<C1> c1);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryAction"]'/>
public delegate void MemoryAction<C0, C1, C2>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryAction"]'/>
public delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryAction"]'/>
public delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryActionU"]'/>
public delegate void MemoryUniformAction<C0, in U>(Memory<C0> c0, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryActionU"]'/>
public delegate void MemoryUniformAction<C0, C1, in U>(Memory<C0> c0, Memory<C1> c1, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryActionU"]'/>
public delegate void MemoryUniformAction<C0, C1, C2, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryActionU"]'/>
public delegate void MemoryUniformAction<C0, C1, C2, C3, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, U uniform);

/// <include file='XMLdoc.xml' path='members/member[@name="T:MemoryActionU"]'/>
public delegate void MemoryUniformAction<C0, C1, C2, C3, C4, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4, U uniform);
