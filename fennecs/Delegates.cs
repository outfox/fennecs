using fennecs.storage;

namespace fennecs;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public delegate void EntityAction(Entity entity);
public delegate void EntitySpanAction(Span<Entity> entities);

public delegate void UniformEntityAction<in U>(U uniform, Entity entity);
public delegate void UniformEntitySpanAction<in U>(U uniform, Span<Entity> entities);

// Future: for inheritance?
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

internal delegate void FennecsAction<in U, in C1, in C2, in C3, in C4>(U uniform, EntityRef entity, C1 accessor1, C2 accessor2, C3 accessor3, C4 accessor4)
    where C1 : allows ref struct
    where C2 : allows ref struct
    where C3 : allows ref struct
    where C4 : allows ref struct
; 

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
