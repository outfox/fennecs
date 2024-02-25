﻿namespace fennecs;

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
public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>, IComparable, IDisposable
{
    #region Internal State

    /// <summary>
    /// Provides a fluent interface for constructing and modifying Entities within a world.
    /// The Entity's Identity is managed internally.
    /// </summary>
    internal Entity(World world, Identity identity)
    {
        _world = world;
        Id = identity;
    }


    /// <summary>
    /// The World in which the Entity exists.
    /// </summary>
    private readonly World _world;

    /// <summary>
    /// The Identity of the Entity.
    /// </summary>
    internal Identity Id { get; }

    #endregion


    #region CRUD

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
    /// <param name="targetEntity">The entity with which to establish the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity AddRelation<B>(Entity targetEntity) where B : notnull, new() => AddRelation(targetEntity, new B());


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
    /// <param name="targetEntity">The entity with which to establish the relation.</param>
    /// <param name="data">The data associated with the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity AddRelation<T>(Entity targetEntity, T data)
    {
        _world.AddRelation(Id, targetEntity.Id, data);
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
    /// <param name="target">The target of the link.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity AddLink<T>(T target) where T : class
    {
        _world.AddLink(Id, target);
        return this;
    }


    /// <summary>
    /// Adds a Component of a specific type, with specific data, to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <param name="data">The data associated with the Component.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>(T data)
    {
        _world.AddComponent(Id, data);
        return this;
    }


    /// <summary>
    /// Adds a Component of a specific type to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Add<T>() where T : new() => Add(new T());


    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be removed.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity Remove<T>()
    {
        _world.RemoveComponent<T>(Id);
        return this;
    }


    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <typeparam name="T">The type of the relation to be removed.</typeparam>
    /// <param name="targetEntity">The entity from which the relation will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity RemoveRelation<T>(Entity targetEntity)
    {
        _world.RemoveRelation<T>(Id, targetEntity);
        return this;
    }


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="targetObject">The target object from which the link will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public Entity RemoveLink<T>(T targetObject) where T : class
    {
        _world.RemoveLink(Id, targetObject);
        return this;
    }


    /// <summary>
    /// Checks if the Entity has a Plain Component.
    /// Same as calling <see cref="Has{T}(Identity)"/> with <see cref="Match.Plain"/>
    /// </summary>
    public bool Has<T>() => _world.HasComponent<T>(Id);


    /// <summary>
    /// Checks if the Entity has a Component of a specific type.
    /// Allows for a <see cref="Match"/> Expression to be specified.
    /// </summary>
    public bool Has<T>(Identity match) => _world.HasComponent<T>(Id, match);


    /// <summary>
    /// Checks if the Entity has an Object Link of a specific type.
    /// </summary>
    public bool HasLink<T>(T targetObject) where T : class => _world.HasLink(Id, targetObject);


    /// <summary>
    /// Checks if the Entity has an Entity-Entity Relation backed by a specific type.
    /// </summary>
    public bool HasRelation<T>(Entity targetEntity) => _world.HasRelation<T>(Id, targetEntity.Id);

    #endregion


    /// <summary>
    /// Disposes of the Entity, releasing any pooled resources.
    /// </summary>
    public void Dispose()
    {
    }


    #region Cast Operators and IEquatable<Entity>

    public bool Equals(Entity other)
    {
        return Id.Equals(other.Id) && Equals(_world, other._world);
    }


    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }


    public override int GetHashCode()
    {
        return HashCode.Combine(_world, Id);
    }


    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }


    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }


    public override string ToString()
    {
        return Id.ToString();
    }


    public int CompareTo(object? obj)
    {
        return obj is Entity other ? CompareTo(other) : 0;
    }


    public int CompareTo(Entity other)
    {
        return Id.CompareTo(other.Id);
    }

    #endregion
}