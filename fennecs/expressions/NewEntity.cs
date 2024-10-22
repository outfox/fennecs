using System.Runtime.InteropServices;

namespace fennecs;

internal static class Bit
{
    internal const ulong StorageMask = 0xF000_0000_0000_0000ul;
    internal const ulong TypeMask = 0x0FFF_0000_0000_0000ul;

    internal const ulong KeyMask = 0x0000_FFFF_FFFF_FFFFul;
    
    // E for Entity, 2 for Object Link, 3 for Keyed Component
    internal const ulong KeyTypeMask = 0x0000_F000_0000_0000ul;

    // For Typed objects (Object Links, Keyed Components)
    internal const ulong SubTypeMask = 0x0000_0FFF_0000_0000ul;

    internal const ulong EntityFlagMask = 0x0000_0F00_0000_0000ul;
    internal const ulong WorldMask = 0x0000_00FF_0000_0000ul;

    
    // Header is Generation in concrete entities, but in Types, it is not needed (as no type may reference a dead entity...? but it might, if stored by user...!)
    internal const ulong HeaderMask = 0xFFFF_0000_0000_0000ul;
    internal const ulong GenerationMask = 0xFFFF_0000_0000_0000ul;
    
    
    internal const ulong Generation = 0x0001_0000_0000_0000ul;
    internal const ulong World = 0x0000_0001_0000_0000ul;
    
    internal const ulong Entity = 0x0000_E000_0000_0000ul;
}


[StructLayout(LayoutKind.Explicit)]
public record struct NewEntity : IAddRemoveComponent<NewEntity>
{
    [FieldOffset(0)]
    internal ulong Value;

    [FieldOffset(0)]
    internal int Index;
    
    [FieldOffset(4)]
    internal byte WorldIndex;

    [FieldOffset(6)]
    internal ushort Generation;
    
    /// <summary>
    /// The World this Entity exists in.
    /// </summary>
    public World _world => World.Get(WorldIndex);

    internal NewEntity(byte worldIndex, int index)
    { 
        Value = Bit.Entity | worldIndex * Bit.World | (uint) index;
    }
    
    internal NewEntity(byte worldIndex, int index, ushort generation)
    { 
        Value = Bit.Entity | generation * Bit.Generation | worldIndex * Bit.World | (uint) index;
    }
    
    /// <summary>
    /// Next Entity in the Generation.
    /// </summary>
    public NewEntity Successor => this with { Generation = (ushort)(Generation + 1) };
    
    #region REFACTOR REMOVEME!
    /// <summary>
    /// Convert to Identity (only for refactoring)
    /// </summary>
    public static implicit operator Identity(NewEntity self) => new(self.Value);
    private Identity Id => new(Value);
    #endregion

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
    public ref C Ref<C>(Match match) where C : struct => ref _world.GetComponent<C>(this, match);


    /// <inheritdoc cref="Ref{C}(fennecs.Match)"/>
    public ref C Ref<C>() => ref _world.GetComponent<C>(this, Match.Plain);

    
    
    /// <summary>
    /// Gets a reference to the Object Link Target of type <typeparamref name="L"/> for the entity.
    /// </summary>
    /// <param name="link">object link match expressioon</param>
    /// <typeparam name="L">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the Entity is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for entity.</exception>
    public ref L Ref<L>(Link<L> link) where L : class => ref _world.GetComponent<L>(this, link);


    /// <inheritdoc />
    public NewEntity Add<T>(Entity relation) where T : notnull, new() => Add(new T(), relation);

    
    /// <inheritdoc cref="Add{R}(R,fennecs.Entity)"/>
    public NewEntity Add<R>(R value, Entity relation) where R : notnull
    {
        _world.AddComponent(Id, TypeExpression.Of<R>(relation), value);
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
    public NewEntity Add<T>(Link<T> link) where T : class
    {
        _world.AddComponent(this, TypeExpression.Of<T>(link), link.Target);
        return this;
    }

    /// <inheritdoc />
    public NewEntity Add<C>() where C : notnull, new() => Add(new C());

    /// <summary>
    /// Adds a Plain Component of a specific type, with specific data, to the current entity. 
    /// </summary>
    /// <param name="data">The data associated with the relation.</param>
    /// <typeparam name="T">Any value or reference component type.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public NewEntity Add<T>(T data) where T : notnull => Add(data, default);
    

    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="C">The type of the Component to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public NewEntity Remove<C>() where C : notnull
    {
        _world.RemoveComponent(this, TypeExpression.Of<C>(Match.Plain));
        return this;
    }

    
    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <param name="relation">target of the relation.</param>
    /// <typeparam name="R">backing type of the relation to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public NewEntity Remove<R>(Entity relation) where R : notnull
    {
        _world.RemoveComponent(this, TypeExpression.Of<R>(new Relate(relation)));
        return this;
    }
    
    /// <inheritdoc />
    public NewEntity Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="link">The target object from which the link will be removed.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public NewEntity Remove<T>(Link<T> link) where T : class
    {
        _world.RemoveComponent(this, link.TypeExpression);
        return this;
    }


    /// <summary>
    /// Despawns the Entity from the World.
    /// </summary>
    /// <remarks>
    /// The entity builder struct still exists afterwards, but the entity is no longer alive and subsequent CRUD operations will throw.
    /// </remarks>
    public void Despawn() => _world.Despawn(this);


    /// <summary>
    /// Checks if the Entity has a Plain Component.
    /// Same as calling <see cref="Has{T}()"/> with <see cref="Identity.Plain"/>
    /// </summary>
    public bool Has<T>() where T : notnull => _world.HasComponent<T>(Id, default);

    
    /// <inheritdoc />
    public bool Has<R>(Entity relation) where R : notnull => _world.HasComponent<R>(Id, new Relate(relation));

    
    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class => Has(Link<L>.With(linkedObject));


    /// <summary>
    /// Checks if the Entity has a Component of a specific type.
    /// Allows for a <see cref="Match"/> Expression to be specified (Wildcards)
    /// </summary>
    public bool Has<T>(Match match) => _world.HasComponent<T>(Id, match);

    /// <summary>
    /// Checks if the Entity has an Object Link of a specific type and specific target.
    /// </summary>
    public bool Has<T>(Link<T> link) where T : class => _world.HasComponent<T>(Id, link);

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
    public IReadOnlyList<Component> Components => _world.GetComponents(Id);
    
    
    /// <summary>
    /// Gets all Components of a specific type and match expression on the Entity.
    /// Supports relation Wildcards, for example:<ul>
    /// <li><see cref="Entity.Any">Entity.Any</see></li>
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
    public T[] Get<T>(Match match) => _world.Get<T>(Id, match);  
    
    #endregion
    
}
