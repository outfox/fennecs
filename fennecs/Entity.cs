// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// <para>
/// <b>Entity</b>
/// </para>
/// <para>
/// Builder Pattern to operate on Identities.
/// Provides a fluent interface for constructing and modifying Entities within a world.
/// The Entity's Identity and World are managed internally.
/// </para>
/// </summary>
/// <remarks>
/// Implements <see cref="IDisposable"/> to later release shared builder resources. Currently a no-op.
/// </remarks>
public readonly record struct Entity : /*IEquatable<Entity>,*/ IAddRemoveComponent<Entity>, IComparable<Entity>, IDisposable
{
    #region Match Expressions

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Target.Any"/>
    public static Target Any => new(Identity.idEntity);
    
    
    #endregion
    
    #region Internal State

    /// <summary>
    /// Provides a fluent interface for constructing and modifying Entities within a world.
    /// The Entity's Identity is managed internally.
    /// </summary>
    internal Entity(World world, Identity identity)
    {
        World = world;
        Id = identity;
    }


    /// <summary>
    /// The World in which the Entity exists.
    /// </summary>
    internal readonly World World;


    /// <summary>
    /// The Identity of the Entity.
    /// </summary>
    internal readonly Identity Id;

    #endregion


    #region CRUD

    /// <summary>
    /// Gets a reference to the Component of type <typeparamref name="C"/> for the entity.
    /// </summary>
    /// <param name="target">specific (targeted) Match Expression for the component type. No wildcards!</param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the Entity is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for entity.</exception>
    public ref C Ref<C>(Target target) => ref World.GetComponent<C>(this, target);


    /// <inheritdoc cref="Ref{C}(Target)"/>
    public ref C Ref<C>() => ref World.GetComponent<C>(this, Identity.Plain);


    /// <summary>
    /// Adds a relation of a specific type, with specific data, between the current entity and the target entity.
    /// The relation is backed by the Component data of the relation. Entities with the same relations are placed
    /// in the same Archetype for faster enumeration and processing as a group.
    ///
    /// The Component data is instantiated / initialized via the default constructor of the relation type.
    /// </summary>
    /// <typeparam name="B">Any value or reference type. The type of the relation to be added.</typeparam>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <param name="relate">The entity with which to establish the relation.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<B>(Relate relate) where B : notnull, new() => Add(new B(), relate);


    /// <summary>
    /// Adds a relation of a specific type, with specific data, between the current entity and the target entity.
    /// The relation is backed by the Component data of the relation. Entities with the same relations are placed
    /// in the same Archetype for faster enumeration and processing as a group.
    /// </summary>
    /// <typeparam name="T">The backing type of the relation to be added, can be any value or reference component type.</typeparam>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <param name="data">The data associated with the relation.</param>
    /// <param name="relate">The entity with which to establish the relation.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>(T data, Relate relate) where T : notnull
    {
        var typeExpression = TypeExpression.Of<T>(relate);
        World.AddComponent(Id, typeExpression, data);
        return this;
    }


    /// <inheritdoc cref="Add{B}(fennecs.Relate)"/>
    public Entity Add<R>(R value, Entity relation) where R : notnull
    {
        Add(value, new Relate(relation));
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
        World.AddComponent(Id, TypeExpression.Of<T>(link), link.Target);
        return this;
    }

    /// <summary>
    /// Adds a Plain Component of a specific type, with specific data, to the current entity. 
    /// </summary>
    /// <param name="data">The data associated with the relation.</param>
    /// <typeparam name="T">Any value or reference component type.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>(T data) where T : notnull => Add(data, default);


    /// <summary>
    /// Adds a newable Component of a specific type to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>() where T : notnull, new() => Add(new T());


    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="C">The type of the Component to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<C>() where C : notnull
    {
        World.RemoveComponent(Id, TypeExpression.Of<C>(Identity.Plain));
        return this;
    }

    
    public Entity Remove<R>(Entity relation) where R : notnull
    {
        World.RemoveComponent(Id, TypeExpression.Of<R>(new Relate(relation)));
        return this;
    }


    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <typeparam name="T">The type of the relation to be removed.</typeparam>
    /// <param name="relation">The entity from which the relation will be removed.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<T>(Relate relation)
    {
        var typeExpression = TypeExpression.Of<T>(relation);
        World.RemoveComponent(Id, typeExpression);
        return this;
    }


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="link">The target object from which the link will be removed.</param>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<T>(Link<T> link) where T : class
    {
        World.RemoveComponent(Id, link.TypeExpression);
        return this;
    }

    public Entity RemoveAny(Match match)
    {
        World.RemoveComponent(Id, match.TypeExpression);
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
    public bool Has<T>() => World.HasComponent<T>(Id, default);


    /// <summary>
    /// Checks if the Entity has a Component of a specific type.
    /// Allows for a <see cref="Cross"/> Expression to be specified.
    /// </summary>
    public bool Has<T>(Target match) => World.HasComponent<T>(Id, match);


    /// <summary>
    /// Checks if the Entity has an Object Link of a specific type and specific target.
    /// </summary>
    public bool Has<T>(Link<T> link) where T : class => World.HasComponent<T>(Id, link);


    /// <summary>
    /// Checks if the Entity has a specifc Entity-Entity Relation backed by a specific type.
    /// </summary>
    public bool Has<T>(Relate relation) => World.HasComponent<T>(Id, relation);

    #endregion


    /// <summary>
    /// Disposes of the Entity builder, releasing any pooled resources.
    /// </summary>
    public void Dispose()
    { }


    #region Cast Operators and IEquatable<Entity>

    /// <inheritdoc />
    public bool Equals(Entity other) => Id.Equals(other.Id) && Equals(World, other.World);

    /// <summary>
    /// Implicit cast to Boolean. Returns true if the Entity is alive and its Identity is nondefault.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static implicit operator bool(Entity entity) => entity.Id && entity.World.IsAlive(entity.Id);


    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(World, Id);


    /// <inheritdoc/>
    public int CompareTo(Entity other) => Id.CompareTo(other.Id);


    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(Id.ToString());
        sb.Append(' ');
        if (World.IsAlive(Id))
        {
            sb.AppendJoin("\n  |-", World.GetSignature(Id));
        }
        else
        {
            sb.Append("|- DEAD");
        }

        return sb.ToString();
    }

    #endregion
}
