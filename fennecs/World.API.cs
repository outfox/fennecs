using System.Collections.Immutable;
using System.Text;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A fennecs.World contains Entities, their Components, compiled Queries, and manages the lifecycles of these objects.
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

    /// <summary>
    /// Spawns a number of pre-configured Entities. 
    /// </summary>
    public EntitySpawner Entity() => new(this);


    /// <summary>
    /// Spawns a number of pre-configured Entities 
    /// </summary>
    /// <remarks>
    /// It's more comfortable to spawn via <see cref="EntitySpawner"/>, from <c>world.Entity()</c>
    /// </remarks>
    /// <param name="components">TypeExpressions and boxed objects to spawn</param>
    /// <param name="count"></param>
    /// <param name="values">component values</param>
    internal void Spawn(int count, IReadOnlyList<TypeExpression> components, IReadOnlyList<object> values)
    {
        var signature = new Signature<TypeExpression>(components.ToImmutableSortedSet()).Add(TypeExpression.Of<Identity>(Match.Plain));
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components, values);
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
    internal void Despawn(Identity identity) => DespawnImpl(identity);


    /// <summary>
    /// Interact with an Identity as an Entity.
    /// Perform operations on the given identity in this world, via fluid API.
    /// </summary>
    /// <example>
    /// <code>world.On(identity).Add(123).Add("string").Remove&lt;int&gt;();</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    [Obsolete("Use Entities instead.")]
    internal Entity On(Identity identity)
    {
        AssertAlive(identity);
        return new(this, identity);
    }


    /// <summary>
    /// Alias for <see cref="On(Identity)"/>, returning an Entity builder struct to operate on. Included to
    /// provide a more intuitive verb to "get" an Entity to assign to a variable.
    /// </summary>
    /// <example>
    /// <code>var bob = world.GetEntity(bobsIdentity);</code>
    /// </example>
    /// <returns>an Entity builder struct whose methods return itself, to provide a fluid syntax. </returns>
    [Obsolete("Use entities instead.")]
    internal Entity GetEntity(Identity identity) => On(identity);
    


    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="identity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    internal bool IsAlive(Identity identity) => identity.IsEntity && identity == _meta[identity.Index].Identity;


    /// <summary>
    /// The number of living entities in the World.
    /// </summary>
    public override int Count => _identityPool.Count;

    /// <summary>
    /// All Queries that exist in this World.
    /// </summary>
    public IReadOnlySet<Query> Queries => _queries;

    #endregion


    #region Bulk Operations

    /// <summary>
    /// Despawn (destroy) all Entities matching a given Type and Match Expression.
    /// </summary>
    /// <typeparam name="T">any component type</typeparam>
    /// <param name="match">default <see cref="Match.Plain"/>.<br/>Can alternatively be one
    /// of <see cref="Match.Any"/>, <see cref="Match.Object"/> or <see cref="Match.Target"/>
    /// </param>
    public void DespawnAllWith<T>(Match match = default)
    {
        var query = Query<Identity>(match).Has<T>(match).Stream();
        query.Raw(delegate(Memory<Identity> entities)
        {
            foreach (var identity in entities.Span) DespawnImpl(identity);
        });
    }


    /// <summary>
    /// Bulk Despawn Entities from a World.
    /// </summary>
    /// <param name="toDelete">the entities to despawn (remove)</param>
    internal void Despawn(ReadOnlySpan<Identity> toDelete)
    {
        lock (_spawnLock)
        {
            for (var i = toDelete.Length - 1; i >= 0; i--)
            {
                DespawnImpl(toDelete[i]);
            }
        }
    }

    /// <summary>
    /// Bulk Despawn Entities from a World.
    /// </summary>
    /// <param name="identities">the entities to despawn (remove)</param>
    internal void Recycle(ReadOnlySpan<Identity> identities)
    {
        lock (_spawnLock)
        {
            foreach (var identity in identities) DespawnDependencies(identity);
            _identityPool.Recycle(identities);
        }
    }

    /// <summary>
    /// Despawn one Entity from a World.
    /// </summary>
    /// <param name="identity">the entity to despawn (remove)</param>
    internal void Recycle(Identity identity)
    {
        lock (_spawnLock)
        {
            DespawnDependencies(identity);
            _identityPool.Recycle(identity);
        }
    }

    #endregion


    #region Lifecycle & Locking

    /// <summary>
    /// Create a new World.
    /// </summary>
    /// <param name="initialCapacity">initial Entity capacity to reserve. The world will grow automatically.</param>
    public World(int initialCapacity = 4096)
    {
        World = this;
       
        
        _identityPool = new(initialCapacity);

        _meta = new Meta[initialCapacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = GetArchetype(new(TypeExpression.Of<Identity>(Match.Plain)));
    }


    /// <summary>
    ///  Runs the World's Garbage Collection (placeholder for future GC - currently removes all empty Archetypes).
    /// </summary>
    public void GC()
    {
        lock (_modeChangeLock)
        {
            if (Mode != WorldMode.Immediate) throw new InvalidOperationException("Cannot run GC while in Deferred mode.");

            foreach (var archetype in Archetypes)
            {
                if (archetype.Count == 0) DisposeArchetype(archetype);
            }

            Archetypes.Clear();
            Archetypes.AddRange(_typeGraph.Values);
        }
    }


    private void DisposeArchetype(Archetype archetype)
    {
        _typeGraph.Remove(archetype.Signature);

        foreach (var type in archetype.Signature)
        {
            _tablesByType[type].Remove(archetype);

            // This is still relevant if ONE relation component is eliminated, but NOT all of them.
            // In the case where the target itself is Despawned, _typesByRelationTarget already
            // had its entire entry for that Target removed.
            if (type.isRelation && _typesByRelationTarget.TryGetValue(type.Identity, out var stillInUse))
            {
                stillInUse.Remove(type);
                if (stillInUse.Count == 0) _typesByRelationTarget.Remove(type.Identity);
            }

            // Same here, if all Archetypes with a Type are gone, we can clear the entry.
            if (_tablesByType[type].Count == 0) _tablesByType.Remove(type);
        }

        foreach (var query in _queries)
        {
            // TODO: Will require some optimization later.
            query.ForgetArchetype(archetype);
        }
    }


    /// <summary>
    /// Disposes of the World. Currently, a no-op.
    /// </summary>
    public new void Dispose()
    {
        //TODO: Dispose all Object Links, Queries, etc.?
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
        sb.AppendLine($" {Archetypes.Count} Archetypes");
        sb.AppendLine($" {Count} Entities");
        sb.AppendLine($" {_queries.Count} Queries");
        sb.AppendLine($"{nameof(WorldMode)}.{Mode}");
        return sb.ToString();
    }

    #endregion
}
