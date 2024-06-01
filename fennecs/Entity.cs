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
public readonly record struct Entity : /*IEquatable<Entity>,*/ IComparable<Entity>, IDisposable
{
    #region Internal State
    /// <summary>
    /// Provides a fluent interface for constructing and modifying Entities within a world.
    /// The Entity's Identity is managed internally.
    /// </summary>
    public Entity(World world, Identity identity)
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
    public readonly Identity Id;
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
    public ref C Ref<C>(Match target) => ref World.GetComponent<C>(this, target);

    
    /// <inheritdoc cref="Ref{C}(fennecs.Identity)"/>
    public ref C Ref<C>() => ref World.GetComponent<C>(this, Match.Plain);


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
    /// <param name="relation">The entity with which to establish the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<B>(Relation relation) where B : notnull, new() => Add(new B(), relation);


    /// <summary>
    /// Adds a relation of a specific type, with specific data, between the current entity and the target entity.
    /// The relation is backed by the Component data of the relation. Entities with the same relations are placed
    /// in the same Archetype for faster enumeration and processing as a group.
    /// </summary>
    /// <typeparam name="T">Any value or reference type. The type of the relation to be added.</typeparam>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <param name="data">The data associated with the relation.</param>
    /// <param name="relation">The entity with which to establish the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>(T data, Relation relation = default) where T : notnull
    {
        var typeExpression = TypeExpression.Of<T>(relation);
        World.AddComponent(Id, typeExpression, data);
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
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>(Link<T> link) where T : class
    {
        World.AddComponent(Id, TypeExpression.Of<T>(link), link.Target);
        return this;
    }


    /*
    /// <summary>
    /// Adds a Component of a specific type, with specific data, to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <param name="data">The data associated with the Component.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>(T data) where T : notnull
    {
        var type = TypeExpression.Of<T>(Match.Plain);
        World.AddComponent(Id, type, data);
        return this;
    }
    */

    /// <summary>
    /// Adds a Component of a specific type to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>() where T : notnull, new() => Add(new T());


    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be removed.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Remove<T>()
    {
        World.RemoveComponent(Id, TypeExpression.Of<T>(Match.Plain));
        return this;
    }


    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <typeparam name="T">The type of the relation to be removed.</typeparam>
    /// <param name="targetEntity">The entity from which the relation will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Remove<T>(Entity targetEntity)
    {
        var typeExpression = TypeExpression.Of<T>(targetEntity);
        World.RemoveComponent(Id, typeExpression);
        return this;
    }


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="link">The target object from which the link will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Remove<T>(Link<T> link) where T : class
    {
        World.RemoveComponent(Id, link);
        return this;
    }


    /// <summary>
    /// Despawns the Entity from the World.
    /// </summary>
    /// <remarks>
    /// The entity builder struct still exists afterwards, but the entity is no longer alive and subsequent CRUD operations will throw.
    /// </remarks>
    public void Despawn() => World.Despawn(Id);


    /// <summary>
    /// Checks if the Entity has a Plain Component.
    /// Same as calling <see cref="Has{T}(Identity)"/> with <see cref="Match.Plain"/>
    /// </summary>
    public bool Has<T>() => World.HasComponent<T>(Id, default);


    /// <summary>
    /// Checks if the Entity has a Component of a specific type.
    /// Allows for a <see cref="Cross"/> Expression to be specified.
    /// </summary>
    public bool Has<T>(Match match) => World.HasComponent<T>(Id, match);


    /// <summary>
    /// Checks if the Entity has an Object Link of a specific type and specific target.
    /// </summary>
    public bool Has<T>(T targetObject) where T : class => World.HasComponent<T>(Id, Link<T>.With(targetObject));


    /// <summary>
    /// Checks if the Entity has an Entity-Entity Relation backed by a specific type.
    /// </summary>
    public bool Has<T>(Entity targetEntity) => World.HasComponent<T>(Id, targetEntity);

    #endregion


    /// <summary>
    /// Disposes of the Entity builder, releasing any pooled resources.
    /// </summary>
    public void Dispose()
    {
    }


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