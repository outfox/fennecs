using System.Collections.Immutable;
using System.Text;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A fennecs.World contains Entities, their Components, and manages their lifecycles.
/// </summary>
public partial class World : IDisposable
{
    #region Entity Spawn, Liveness, and Despawn
    /// <summary>
    /// Creates a new Identity in this World, and returns its Entity builder struct.
    /// Reuses previously despawned Entities, whose Identities will differ in Generation after respawn. 
    /// </summary>
    /// <returns>an Entity to operate on</returns>
    public Entity Spawn() => new(this, NewEntity()); //TODO: Check if semantically legal to spawn in Deferred mode.

    
    internal PooledList<Identity> SpawnBare(int count)
    {
        var identities = _identityPool.Spawn(count);
        while (_meta.Length <= _identityPool.Created) Array.Resize(ref _meta, _meta.Length * 2);
        return identities;
    }

    /*

    /// <summary>
    /// Spawns a number of pre-configured Entities 
    /// </summary>
    /// <param name="components">TypeExpressions and boxed objects to spawn</param>
    /// <param name="count"></param>
    public void Spawn(int count = 1, params object[] components)
    {
        var typeSet = components.Select(c => TypeExpression.Of(c.GetType())).Append(TypeExpression.Of<Identity>()).ToImmutableSortedSet();
        var signature = new Signature<TypeExpression>(typeSet);
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components);
    }
    */

    /// <summary>
    /// Spawns a number of pre-configured Entities 
    /// </summary>
    /// <remarks>
    /// It's more comfortable to spawn via <see cref="Query.Spawn"/>
    /// </remarks>
    public EntitySpawner Entity()
    {
        return new EntitySpawner(this);
        /*
        var signature = new Signature<TypeExpression>(components.Select(c => c.Item1).Append(TypeExpression.Of<Identity>()).ToImmutableSortedSet());
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components.Select(c => c.Item2).ToArray());
        */
    }


    /// <summary>
    /// Spawns a number of pre-configured Entities 
    /// </summary>
    /// <remarks>
    /// It's more comfortable to spawn via <see cref="Query.Spawn"/>
    /// </remarks>
    /// <param name="components">TypeExpressions and boxed objects to spawn</param>
    /// <param name="count"></param>
    /// <param name="values">component values</param>
    internal void Spawn(int count, IReadOnlyList<TypeExpression> components, IReadOnlyList<object> values)
    {
        var signature = new Signature<TypeExpression>(components.ToImmutableSortedSet()).Add(TypeExpression.Of<Identity>());
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components, values);
    }


    /// <summary>
    /// Spawns a number of pre-configured Entities 
    /// </summary>
    /// <remarks>
    /// It's more comfortable to spawn via <see cref="Query.Spawn"/>
    /// </remarks>
    /// <param name="components">TypeExpressions and boxed objects to spawn</param>
    /// <param name="count"></param>
    public void Spawn(int count = 1, params (TypeExpression, object)[] components)
    {
        var signature = new Signature<TypeExpression>(components.Select(c => c.Item1).Append(TypeExpression.Of<Identity>()).ToImmutableSortedSet());
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components.Select(c => c.Item2).ToArray());
    }


    /// <summary>
    /// Despawn (destroy) an Entity from this World.
    /// </summary>
    /// <param name="entity">the entity to despawn.</param>
    public void Despawn(Entity entity) => DespawnImpl(entity.Id);


    /// <summary>
    /// Despawn (destroy) an Entity from this World by its Identity.
    /// </summary>
    /// <param name="identity">the entity to despawn.</param>
    public void Despawn(Identity identity) => DespawnImpl(identity);


    /// <summary>
    /// Interact with an Identity as an Entity.
    /// Perform operations on the given identity in this world, via fluid API.
    /// </summary>
    /// <example>
    /// <code>world.On(identity).Add(123).Add("string").Remove&lt;int&gt;();</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    public Entity On(Identity identity)
    {
        AssertAlive(identity);
        return new Entity(this, identity);
    }


    /// <summary>
    /// Alias for <see cref="On(Identity)"/>, returning an Entity builder struct to operate on. Included to
    /// provide a more intuitive verb to "get" an Entity to assign to a variable.
    /// </summary>
    /// <example>
    /// <code>var bob = world.GetEntity(bobsIdentity);</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    public Entity GetEntity(Identity identity) => On(identity);


    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="identity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    public bool IsAlive(Identity identity) => identity.IsEntity && identity == _meta[identity.Index].Identity;


    /// <summary>
    /// The number of living entities in the World.
    /// </summary>
    public int Count => _identityPool.Count;
    #endregion


    #region Bulk Operations
    /// <summary>
    /// Despawn (destroy) all Entities matching a given Type and Match Expression.
    /// </summary>
    /// <typeparam name="T">any component type</typeparam>
    /// <param name="match">default <see cref="Match.Plain"/>.<br/>Can alternatively be one
    /// of <see cref="Match.Any"/>, <see cref="Match.Object"/> or <see cref="Match.Target"/>
    /// </param>
    public void DespawnAllWith<T>(Identity match = default)
    {
        using var query = Query<Identity>().Has<T>(match).Build();
        query.Raw(delegate(Memory<Identity> entities)
        {
            foreach (var identity in entities.Span) DespawnImpl(identity);
        });
    }


    /// <summary>
    /// Bulk Despawn Entities from a World.
    /// </summary>
    /// <param name="toDelete">the entities to despawn (remove)</param>
    public void Despawn(ReadOnlySpan<Identity> toDelete)
    {
        foreach (var identity in toDelete)
        {
            DespawnImpl(identity);
        }
    }
    #endregion


    #region Lifecycle & Locking
    /// <summary>
    /// Create a new World.
    /// </summary>
    /// <param name="capacity">initial Entity capacity to reserve. The world will grow automatically.</param>
    public World(int capacity = 4096)
    {
        _identityPool = new IdentityPool(capacity);

        _meta = new Meta[capacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = GetArchetype(new Signature<TypeExpression>(TypeExpression.Of<Identity>(Match.Plain)));
    }


    /// <summary>
    ///  Runs the World's Garbage Collection (placeholder for future GC - currently removes all empty Archetypes).
    /// </summary>
    public void GC()
    {
        lock (_modeChangeLock)
        {
            if (Mode != WorldMode.Immediate) throw new InvalidOperationException("Cannot run GC while in Deferred mode.");

            foreach (var archetype in _archetypes)
            {
                if (archetype.Count == 0) ForgetArchetype(archetype);
            }

            _archetypes.Clear();
            _archetypes.AddRange(_typeGraph.Values);
        }
    }


    private void ForgetArchetype(Archetype archetype)
    {
        _typeGraph.Remove(archetype.Signature);

        foreach (var type in archetype.Signature)
        {
            _tablesByType[type].Remove(archetype);
            if (type.isRelation) _typesByRelationTarget[type.Target].Remove(type);

            if (_tablesByType[type].Count == 0) _tablesByType.Remove(type);
            if (type.isRelation && _typesByRelationTarget[type.Target].Count == 0) _typesByRelationTarget.Remove(type.Target);
        }

        foreach (var query in _queries.Values)
        {
            // TODO: Will require some optimization later.
            query.ForgetArchetype(archetype);
        }
    }


    /// <summary>
    /// Disposes of the World. Currently, a no-op.
    /// </summary>
    public void Dispose()
    {
        //TODO: Release all Object Links?
    }


    /// <summary>
    /// Locks the World (setting into a Deferred mode) for the scope of the returned WorldLock.
    /// Multiple Locks can be taken out, and all structural Operations on Entities will be queued,
    /// and executed once the last Lock is released. 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public WorldLock Lock() => new(this);

    #endregion
    
    #region Debug Tools

    /// <inheritdoc />
    public override string ToString()
    {
        return DebugString();
    }

    /// <inheritdoc cref="ToString"/>
    private string DebugString()
    {
        var sb = new StringBuilder("World:");
        sb.AppendLine();
        sb.AppendLine($" {_archetypes.Count} Archetypes");
        sb.AppendLine($" {Count} Entities");
        sb.AppendLine($" {_queries.Count} Queries");
        sb.AppendLine($"{nameof(WorldMode)}.{Mode}");
        return sb.ToString();
    }

    #endregion

public class EntityBatch : IDisposable
{
    private readonly World _world;
    private PooledList<Identity> _identities = PooledList<Identity>.Rent();
    private Signature<TypeExpression> _signature;
    private Dictionary<TypeExpression, object> _components;

    internal EntityBatch(World world, int count)
    {
        _world = world;
        _identities.AddRange(_world._identityPool.Spawn(count));
    }

    public void Submit()
    {
        
    }
    public void Dispose()
    {
        _identities.Dispose();
    }
}
}
