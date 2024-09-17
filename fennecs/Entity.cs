﻿// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// <para>
/// <b>EntityOld</b>
/// </para>
/// <para>
/// Builder Pattern to operate on Identities.
/// Provides a fluent interface for constructing and modifying Entities within a world.
/// The EntityOld's Identity and World are managed internally.
/// </para>
/// </summary>
/// <remarks>
/// Implements <see cref="IDisposable"/> to later release shared builder resources. Currently a no-op.
/// </remarks>
public readonly record struct EntityOld : IAddRemoveComponent<EntityOld>, IHasComponent, IComparable<EntityOld>
{
    #region Match Expressions

    /// <summary>
    /// <para><b>Wildcard match expression for EntityOld iteration.</b><br/>This matches only <b>EntityOld-EntityOld</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Match.Any"/>
    public static Match Any => new(Identity.EntityOld);
    
    #endregion
    
    #region Internal State

    /// <summary>
    /// Provides a fluent interface for constructing and modifying Entities within a world.
    /// The EntityOld's Identity is managed internally.
    /// </summary>
    internal EntityOld(World world, Identity identity)
    {
        _world = world;
        Id = identity;
    }


    /// <summary>
    /// The World in which the EntityOld exists.
    /// </summary>
    internal readonly World _world;


    /// <summary>
    /// The Identity of the EntityOld.
    /// </summary>
    internal readonly Identity Id;

    #endregion


    #region CRUD

    /// <summary>
    /// Gets a reference to the Component of type <typeparamref name="C"/> for the EntityOld.
    /// </summary>
    /// <remarks>
    /// Adds the component before if possible.
    /// </remarks>
    /// <param name="match">specific (targeted) Match Expression for the component type. No wildcards!</param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the EntityOld is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for EntityOld.</exception>
    public ref C Ref<C>(Match match) where C : struct => ref _world.GetComponent<C>(Id, match);


    /// <inheritdoc cref="Ref{C}(fennecs.Match)"/>
    public ref C Ref<C>() => ref _world.GetComponent<C>(Id, Match.Plain);

    
    
    /// <summary>
    /// Gets a reference to the Object Link Target of type <typeparamref name="L"/> for the EntityOld.
    /// </summary>
    /// <param name="link">object link match expressioon</param>
    /// <typeparam name="L">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="ObjectDisposedException">If the EntityOld is not Alive..</exception>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the World's tables for EntityOld.</exception>
    public ref L Ref<L>(Link<L> link) where L : class => ref _world.GetComponent<L>(Id, link);


    /// <inheritdoc />
    public EntityOld Add<T>(EntityOld relation) where T : notnull, new() => Add(new T(), relation);

    
    /// <inheritdoc cref="Add{R}(R,fennecs.EntityOld)"/>
    public EntityOld Add<R>(R value, EntityOld relation) where R : notnull
    {
        _world.AddComponent(Id, TypeExpression.Of<R>(relation), value);
        return this;
    }

    /// <summary>
    /// Adds a object link to the current EntityOld.
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
    /// <typeparam name="T">Any reference type. The type the object to be linked with the EntityOld.</typeparam>
    /// <param name="link">The target of the link.</param>
    /// <returns>EntityOld struct itself, allowing for method chaining.</returns>
    public EntityOld Add<T>(Link<T> link) where T : class
    {
        _world.AddComponent(Id, TypeExpression.Of<T>(link), link.Target);
        return this;
    }

    /// <inheritdoc />
    public EntityOld Add<C>() where C : notnull, new() => Add(new C());

    /// <summary>
    /// Adds a Plain Component of a specific type, with specific data, to the current EntityOld. 
    /// </summary>
    /// <param name="data">The data associated with the relation.</param>
    /// <typeparam name="T">Any value or reference component type.</typeparam>
    /// <returns>EntityOld struct itself, allowing for method chaining.</returns>
    public EntityOld Add<T>(T data) where T : notnull => Add(data, default);
    

    /// <summary>
    /// Removes a Component of a specific type from the current EntityOld.
    /// </summary>
    /// <typeparam name="C">The type of the Component to be removed.</typeparam>
    /// <returns>EntityOld struct itself, allowing for method chaining.</returns>
    public EntityOld Remove<C>() where C : notnull
    {
        _world.RemoveComponent(Id, TypeExpression.Of<C>(Match.Plain));
        return this;
    }

    
    /// <summary>
    /// Removes a relation of a specific type between the current EntityOld and the target EntityOld.
    /// </summary>
    /// <param name="relation">target of the relation.</param>
    /// <typeparam name="R">backing type of the relation to be removed.</typeparam>
    /// <returns>EntityOld struct itself, allowing for method chaining.</returns>
    public EntityOld Remove<R>(EntityOld relation) where R : notnull
    {
        _world.RemoveComponent(Id, TypeExpression.Of<R>(new Relate(relation.Id)));
        return this;
    }
    
    /// <inheritdoc />
    public EntityOld Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));


    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="link">The target object from which the link will be removed.</param>
    /// <returns>EntityOld struct itself, allowing for method chaining.</returns>
    public EntityOld Remove<T>(Link<T> link) where T : class
    {
        _world.RemoveComponent(Id, link.TypeExpression);
        return this;
    }


    /// <summary>
    /// Despawns the EntityOld from the World.
    /// </summary>
    /// <remarks>
    /// The EntityOld builder struct still exists afterwards, but the EntityOld is no longer alive and subsequent CRUD operations will throw.
    /// </remarks>
    public void Despawn() => _world.Despawn(this);


    /// <summary>
    /// Checks if the EntityOld has a Plain Component.
    /// Same as calling <see cref="Has{T}()"/> with <see cref="Identity.Plain"/>
    /// </summary>
    public bool Has<T>() where T : notnull => _world.HasComponent<T>(Id, default);

    
    /// <inheritdoc />
    public bool Has<R>(EntityOld relation) where R : notnull => _world.HasComponent<R>(Id, new Relate(relation.Id));

    
    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class => Has(Link<L>.With(linkedObject));


    /// <summary>
    /// Checks if the EntityOld has a Component of a specific type.
    /// Allows for a <see cref="Match"/> Expression to be specified (Wildcards)
    /// </summary>
    public bool Has<T>(Match match) => _world.HasComponent<T>(Id, match);

    /// <summary>
    /// Checks if the EntityOld has an Object Link of a specific type and specific target.
    /// </summary>
    public bool Has<T>(Link<T> link) where T : class => _world.HasComponent<T>(Id, link);

    /// <summary>
    /// Boxes all the Components on the EntityOld into an array.
    /// Use sparingly, but don't be paranoid. Suggested uses: serialization and debugging.
    /// </summary>
    /// <remarks>
    /// Values and References are copied, changes to the array will not affect the EntityOld.
    /// Changes to objects in the array will affect these objects in the World.
    /// This array is re-created every time this getter is called.
    /// The values are re-boxed each time this getter is called.
    /// </remarks>
    public IReadOnlyList<Component> Components => _world.GetComponents(Id);
    
    
    /// <summary>
    /// Gets all Components of a specific type and match expression on the EntityOld.
    /// Supports relation Wildcards, for example:<ul>
    /// <li><see cref="EntityOld.Any">EntityOld.Any</see></li>
    /// <li><see cref="Link.Any">Link.Any</see></li>
    /// <li><see cref="Match.Target">Match.Target</see></li>
    /// <li><see cref="Match.Any">Match.Any</see></li>
    /// <li><see cref="Match.Plain">Match.Plain</see></li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// This is not intended as the main way to get a component from an EntityOld. Consider <see cref="Stream"/>s instead.
    /// </remarks>
    /// <param name="match">match expression, supports wildcards</param>
    /// <typeparam name="T">backing type of the component</typeparam>
    /// <returns>array with all the component values stored for this EntityOld</returns>
    public T[] Get<T>(Match match) => _world.Get<T>(Id, match);  
    
    #endregion


    #region Cast Operators and IEquatable<EntityOld>

    /// <summary>
    /// Temporary implicit cast to Identity. (before types get fused)
    /// </summary>
    public static implicit operator Identity(EntityOld self) => self.Id;

    /// <summary>
    /// True if the EntityOld is alive in its world (and has a world).
    /// </summary>
    public static implicit operator bool(EntityOld EntityOld) => EntityOld.Alive;
    
    /// <inheritdoc />
    public bool Equals(EntityOld other) => Id.Equals(other.Id) && Equals(_world, other._world);
    

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_world, Id);


    /// <inheritdoc/>
    public int CompareTo(EntityOld other) => Id.CompareTo(other.Id);


    /// <summary>
    /// Is this EntityOld Alive in its World?
    /// </summary>
    public bool Alive => _world != null && _world.IsAlive(Id);

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(Id.ToString());
        sb.Append(' ');
        if (_world.IsAlive(Id))
        {
            sb.AppendJoin("\n  |-", _world.GetSignature(Id));
        }
        else
        {
            sb.Append("-DEAD-");
        }

        return sb.ToString();
    }
    
    #endregion
}
