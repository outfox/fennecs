using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace fennecs;

internal static class Bit
{
    internal const ulong StorageMask = 0xF000_0000_0000_0000ul;
    internal const ulong TypeMask = 0x0FFF_0000_0000_0000ul;

    internal const ulong KeyMask = 0x0000_FFFF_FFFF_FFFFul;
    
    // 1 for Entity, 2 for Object Link, 4 for Keyed Component
    internal const ulong KeyTypeMask = 0x0000_F000_0000_0000ul;

    internal const ulong KeyNone = 0x0000_0000_0000_0000ul;
    internal const ulong KeyAny = 0x0000_F000_0000_0000ul;

    internal const ulong KeyWild = 0x0000_1000_0000_0000ul;
    internal const ulong KeyTarget = 0x0000_2000_0000_0000ul;
    internal const ulong KeyObject = 0x0000_7000_0000_0000ul;
    internal const ulong KeyEntity = Entity;

    // For Typed objects (Object Links, Keyed Components)
    internal const ulong SubTypeMask = 0x0000_0FFF_0000_0000ul;

    
    internal const ulong EntityFlagWild = 0x0000_0F00_0000_0000ul;
    
    internal const ulong EntityFlagMask = 0x0000_0F00_0000_0000ul;
    internal const ulong WorldMask = 0x0000_00FF_0000_0000ul;

    
    // Header is Generation in concrete entities, but in Types, it is not needed (as no type may reference a dead entity...? but it might, if stored by user...!)
    internal const ulong HeaderMask = 0xFFFF_0000_0000_0000ul;
    internal const ulong GenerationMask = 0xFFFF_0000_0000_0000ul;
    
    
    internal const ulong Generation = 0x0001_0000_0000_0000ul;
    internal const ulong World = 0x0000_0001_0000_0000ul;
    
    internal const ulong Entity = 0x0000_E000_0000_0000ul;
}


/// <summary>
/// Entity is a struct that is associated with a world and a specific index in it. Components may be added or removed from the Entity.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct Entity : IAddRemoveComponent<Entity>, IHasComponent
{
    [FieldOffset(0)]
    internal readonly ulong Value;

    // 0x0000_E000_####_####
    [FieldOffset(0)]
    internal readonly int Index;
    
    // 0x0000_E0##_0000_0000
    [FieldOffset(4)]
    internal readonly byte WorldIndex;

    // 0x####_E000_0000_0000
    [FieldOffset(6)]
    internal readonly ushort Generation;
    
    /// <summary>
    /// A Key that matches relations with any Entity.
    /// </summary>
    /// <remarks>
    /// This excludes plain components, i.e. which have no relation target.
    /// </remarks>
    public static Key Any = new(Bit.KeyEntity | Bit.EntityFlagWild);
    
    
    /// <summary>
    /// The Key of this Entity, used in Relations.
    /// </summary>
    public static implicit operator Key(Entity self)
    {
        if (!self.Alive) throw new InvalidOperationException("Entity is not alive.");
        return new Key(self.Value & Bit.KeyMask);
    }

    /// <summary>
    /// The World this Entity exists in.
    /// </summary>
    private World World => World.Get(WorldIndex);

    internal Entity(byte worldIndex, int index)
    { 
        Value = Bit.Entity | worldIndex * Bit.World | (uint) index;
    }
    
    internal Entity(byte worldIndex, int index, ushort generation)
    { 
        Value = Bit.Entity | generation * Bit.Generation | worldIndex * Bit.World | (uint) index;
    }
    
    /// <summary>
    /// Next Entity in the Generation.
    /// </summary>
    public Entity Successor => new(WorldIndex, Index,(ushort) (Generation + 1));
    
    #region CRUD

    /// <summary>
    /// Gets a reference to the Component of type <typeparamref name="C"/> for the entity.
    /// </summary>
    /// <remarks>
    /// Adds the component before if possible.
    /// </remarks>
    /// <param name="match">specific (targeted) Match Expression for the component type. No wildcards!</param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the Entity is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for entity.</exception>
    public ref C Ref<C>(Match match) where C : struct => ref World.GetComponent<C>(this, match);


    /// <inheritdoc cref="Ref{C}(fennecs.Match)"/>
    public ref C Ref<C>() => ref World.GetComponent<C>(this, Match.Plain);

    
    
    /// <summary>
    /// Gets a reference to the Object Link Target of type <typeparamref name="L"/> for the entity.
    /// </summary>
    /// <param name="link">object link match expressioon</param>
    /// <typeparam name="L">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the Entity is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for entity.</exception>
    public ref L Ref<L>(Link<L> link) where L : class => ref World.GetComponent<L>(this, link);


    /// <inheritdoc />
    public Entity Add<T>(Entity relation) where T : notnull, new() => Add(new T(), relation);

    
    /// <inheritdoc cref="Add{R}(R,fennecs.Entity)"/>
    public Entity Add<R>(R value, Entity relation) where R : notnull
    {
        World.AddComponent(this, TypeExpression.Of<R>(relation), value);
        return this;
    }

    /// <summary>
    /// Adds a object link to the current entity.
    /// Object links, in addition to making the object available as a Component,
    /// place all Entities with a link to the same object into a single Archetype,
    /// which can optimize processing them in queries.
    /// </summary>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <typeparam name="T">Any reference type. The type the object to be linked with the entity.</typeparam>
    /// <param name="link">The target of the link.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>(Link<T> link) where T : class
    {
        World.AddComponent(this, TypeExpression.Of<T>(link), link.Target);
        return this;
    }

    /// <inheritdoc />
    public Entity Add<C>() where C : notnull, new() => Add(new C());

    /// <summary>
    /// Adds a Plain Component of a specific type, with specific data, to the current entity. 
    /// </summary>
    /// <param name="data">The data associated with the relation.</param>
    /// <typeparam name="T">Any value or reference component type.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>(T data) where T : notnull => Add(data, default);
    

    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="C">The type of the Component to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<C>() where C : notnull
    {
        World.RemoveComponent(this, TypeExpression.Of<C>(Match.Plain));
        return this;
    }

    
    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <param name="relation">target of the relation.</param>
    /// <typeparam name="R">backing type of the relation to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<R>(Entity relation) where R : notnull
    {
        World.RemoveComponent(this, TypeExpression.Of<R>(new Relate(relation)));
        return this;
    }
    
    /// <inheritdoc />
    public Entity Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="link">The target object from which the link will be removed.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<T>(Link<T> link) where T : class
    {
        World.RemoveComponent(this, link.TypeExpression);
        return this;
    }


    /// <summary>
    /// Despawns the Entity from the World.
    /// </summary>
    /// <remarks>
    /// The entity builder struct still exists afterwards, but the entity is no longer alive and subsequent CRUD operations will throw.
    /// </remarks>
    public void Despawn() => World.Despawn(this);


    /// <summary>
    /// Checks if the Entity has a Plain Component.
    /// Same as calling <see cref="Has{T}()"/> with <see cref="Identity.Plain"/>
    /// </summary>
    public bool Has<T>() where T : notnull => World.HasComponent<T>(this, default);

    
    /// <inheritdoc />
    public bool Has<R>(Entity relation) where R : notnull => World.HasComponent<R>(this, new Relate(relation));

    
    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class => Has(Link<L>.With(linkedObject));


    /// <summary>
    /// Checks if the Entity has a Component of a specific type.
    /// Allows for a <see cref="Match"/> Expression to be specified (Wildcards)
    /// </summary>
    public bool Has<T>(Match match) => World.HasComponent<T>(this, match);

    /// <summary>
    /// Checks if the Entity has an Object Link of a specific type and specific target.
    /// </summary>
    public bool Has<T>(Link<T> link) where T : class => World.HasComponent<T>(this, link);

    /// <summary>
    /// Boxes all the Components on the entity into an array.
    /// Use sparingly, but don't be paranoid. Suggested uses: serialization and debugging.
    /// </summary>
    /// <remarks>
    /// Values and References are copied, changes to the array will not affect the Entity.
    /// Changes to objects in the array will affect these objects in the World.
    /// This array is re-created every time this getter is called.
    /// The values are re-boxed each time this getter is called.
    /// </remarks>
    public IReadOnlyList<Component> Components => World.GetComponents(this);

    /// <summary>
    /// Is this Entity alive in its World?
    /// </summary>
    /// <remarks>
    /// Invalid or uninitialized Entities are considered dead (not Alive).
    /// </remarks>
    public bool Alive => (Value & Bit.Entity) == Bit.Entity && World.IsAlive(this);
    
    /// <summary>
    /// Gets all Components of a specific type and match expression on the Entity.
    /// Supports relation Wildcards, for example:<ul>
    /// <li><see cref="JSType.Any">Entity.Any</see></li>
    /// <li><see cref="Link.Any">Link.Any</see></li>
    /// <li><see cref="Match.Target">Match.Target</see></li>
    /// <li><see cref="Match.Any">Match.Any</see></li>
    /// <li><see cref="Match.Plain">Match.Plain</see></li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// This is not intended as the main way to get a component from an entity. Consider <see cref="Stream"/>s instead.
    /// </remarks>
    /// <param name="match">match expression, supports wildcards</param>
    /// <typeparam name="T">backing type of the component</typeparam>
    /// <returns>array with all the component values stored for this entity</returns>
    public T[] Get<T>(Match match) => World.Get<T>(this, match);  
    
    #endregion
    
}
