// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using fennecs.CRUD;
using fennecs.storage;

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
public readonly record struct Entity : IAddRemove<Entity>, IHasTyped, IAddRemoveBoxed<Entity>, IComparable<Entity>
{
    #region Match Expressions

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Match.Any"/>
    public static Match Any => new(Identity.Entity);

    #endregion

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
    // ReSharper disable once InconsistentNaming
    internal readonly World _world;


    /// <summary>
    /// The Identity of the Entity.
    /// </summary>
    internal readonly Identity Id;

    #endregion


    #region IAddRemoveComponent

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
    public ref C Ref<C>(Match match = default) where C : notnull => ref _world.GetComponent<C>(this, match);

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
    
    /// <inheritdoc cref="Ref{C}(fennecs.Match)"/>
    public RWImmediate<C> RW<C>(Match match = default) where C : notnull
    {
        return new(ref _world.GetComponent<C>(this, match), this, match);
    }


    /// <inheritdoc />
    public Entity Add<T>(Entity relation) where T : notnull, new() => Add(new T(), relation);


    /// <inheritdoc cref="Add{R}(R,fennecs.Entity)"/>
    public Entity Add<CR>(CR component, Entity relation) where CR : notnull
    {
        _world.AddComponent(Id, TypeExpression.Of<CR>(relation), component);
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
        _world.AddComponent(Id, TypeExpression.Of<T>(link), link.Target);
        return this;
    }

    /// <inheritdoc />
    public Entity Add<C>() where C : notnull, new() => Add(new C());

    /// <summary>
    /// Adds a Plain Component of a specific type, with specific data, to the current entity. 
    /// </summary>
    /// <param name="component">The data associated with the relation.</param>
    /// <typeparam name="T">Any value or reference component type.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Add<T>(T component) where T : notnull => Add(component, default);


    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="C">The type of the Component to be removed.</typeparam>
    /// <returns>Entity struct itself, allowing for method chaining.</returns>
    public Entity Remove<C>(Match match = default) where C : notnull
    {
        _world.RemoveComponent(Id, TypeExpression.Of<C>(match));
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
        _world.RemoveComponent(Id, TypeExpression.Of<R>(new Relate(relation)));
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
        _world.RemoveComponent(Id, link.TypeExpression);
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
    public T[] Get<T>(Match match) where T : notnull => _world.Get<T>(Id, match);

    #endregion

    #region IHasComponent

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
    
    #endregion
    
    #region IBoxedComponent

    /// <inheritdoc />
    public bool Has(Type type, Match match) => _world.HasComponent(this, TypeExpression.Of(type, match));
    
    

    /// <inheritdoc />
    public bool Get([MaybeNullWhen(false)] out object value, Type type, Match match = default)
    {
        return _world.GetComponent(this, TypeExpression.Of(type, match), out value);
    }


    /// <inheritdoc />
    public object? Get(Type type, Match match = default)
    {
        return _world.GetComponent(this, TypeExpression.Of(type, match), out var value) ? value : null;
    }


    /// <inheritdoc />
    public void Set(object value, Match match = default)
    {
        _world.AddComponent(this, TypeExpression.Of(value.GetType(), match), value);
    }

    
    /// <inheritdoc />
    public Entity Clear(Type type, Match match = default)
    {
        var expression = TypeExpression.Of(type, match);
        
        if (!match.IsWildcard)
        {
            _world.RemoveComponent(this, expression);
            return this;
        }
        
        var components = Components.Where(c => expression.Matches(c.Expression)).ToArray();
        foreach (var component in components)
        {
            _world.RemoveComponent(this, component.Expression);
        }
        return this;
    }
    
    #endregion
    
    #region Cast Operators and IEquatable<Entity>

    /// <summary>
    /// True if the Entity is alive in its world (and has a world).
    /// </summary>
    public static implicit operator bool(Entity entity) => entity.Alive;

    /// <inheritdoc />
    public bool Equals(Entity other) => Id.Equals(other.Id) && Equals(_world, other._world);


    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_world, Id);


    /// <inheritdoc/>
    public int CompareTo(Entity other) => Id.CompareTo(other.Id);


    /// <summary>
    /// Is this Entity Alive in its World?
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
