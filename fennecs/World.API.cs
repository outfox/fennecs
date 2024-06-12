using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A fennecs.World contains Entities, their Components, compiled Queries, and manages the lifecycles of these objects.
/// </summary>
public partial class World : IDisposable
{
    #region Config
        /// <summary>
        /// Optional name for the World.
        /// </summary>
        public string Name { get; init; }
        
        /// <summary>
        /// Flags denoting this World's Garbage Collection Strategy.
        /// </summary>
        public GCAction GCBehaviour { get; init; } = GCAction.DefaultBeta;
    #endregion
    
    /// <summary>
    /// Flags to compose Garbage Collection Strategies.
    /// </summary>
    [Flags]
    public enum GCAction
    {
        /// <summary>
        /// Default GC Strategy for the beta phase.
        /// </summary>
        DefaultBeta = ManualOnly | CompactStagnantArchetypes | DisposeEmptyRelationArchetypes,
        
        /// <summary>
        /// Do nothing.
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// Compact Archetypes when invoked.
        /// </summary>
        CompactStagnantArchetypes = 1,
        /// <summary>
        /// Dispose of empty Relation Archetypes when invoked.
        /// </summary>
        DisposeEmptyRelationArchetypes = 2,
        /// <summary>
        /// Dispose of empty Archetypes when invoked.
        /// </summary>
        DisposeEmptyArchetypes = 4,
        /// <summary>
        /// Compact the Meta Table
        /// </summary>
        CompactMeta = 8,
        
        /// <summary>
        /// No Automatic GC, call World.GC() manually.
        /// </summary>
        ManualOnly = 0,
        /// <summary>
        /// Invoke GC on World.Catchup.
        /// </summary>
        InvokeOnWorldCatchup = 128,
        /// <summary>
        /// Invoke GC on every Single Entity Despawn. 
        /// </summary>
        InvokeOnSingleDespawn = 256,
        /// <summary>
        /// Invoke GC on Entity Bulk Despawn (includes <c>Truncate</c>, <c>Clear</c>)
        /// </summary>
        InvokeOnBulkDespawn = 512,
    }
    
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
        Array.Resize(ref _meta, (int) BitOperations.RoundUpToPowerOf2((uint)_identityPool.Created + 1));
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
        var signature = new Signature(components.ToImmutableSortedSet()).Add(Component.PlainComponent<Identity>().value);
        var archetype = GetArchetype(signature);
        archetype.Spawn(count, components, values);
    }

    /// <summary>
    /// Despawn (destroy) an Entity from this World.
    /// </summary>
    /// <param name="entity">the entity to despawn.</param>
    public void Despawn(Entity entity) => DespawnImpl(entity);

    
    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="identity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    internal bool IsAlive(Identity identity) => identity == _meta[identity.Index].Identity;


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
    /// of <see cref="Match.Any"/>, <see cref="Match.Target"/>, or <see cref="Match.Object"/>, <see cref="Match.Entity"/>
    /// </param>
    public void DespawnAllWith<T>(Match match = default)
    {
        var query = Query<Identity>().Has<T>(match).Stream();
        query.Raw(delegate(Memory<Identity> entities)
        {
            //TODO: This is not good. Need to untangle the types here.
            foreach (var identity in entities.Span) DespawnImpl(new(this, identity));
        });
    }


    /// <summary>
    /// Bulk Despawn Entities from a World.
    /// </summary>
    /// <param name="toDelete">the entities to despawn (remove)</param>
    internal void Despawn(ReadOnlySpan<Entity> toDelete)
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
    /// Bulk Recycle Entities from a World.
    /// </summary>
    /// <remarks>
    /// MUST BE REMOVED FROM ITS ARCHETYPE STORAGE! (used by Archetype.Truncate)
    /// </remarks>
    /// <param name="identities">the entities to despawn (remove)</param>
    internal void Recycle(ReadOnlySpan<Identity> identities)
    {
        lock (_spawnLock)
        {
            //TODO: Not good to assemble the Entity like that. Types need to be untangled.
            foreach (var identity in identities)
            {
                DespawnDependencies(new(this, identity));
            }
            foreach (var identity in identities)_meta[identity.Index] = default;

            _identityPool.Recycle(identities);
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
        Name = nameof(World);
        
        World = this;
       
        _identityPool = new(initialCapacity);

        _meta = new Meta[initialCapacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = GetArchetype(new(Component.PlainComponent<Identity>().value));
    }


    /// <summary>
    ///  Runs the World's Garbage Collection (placeholder for future GC - currently removes all empty Archetypes).
    /// </summary>
    public void GC()
    {
        lock (_modeChangeLock)
        {
            if (Mode != WorldMode.Immediate) throw new InvalidOperationException("Cannot run GC while in Deferred mode.");

            foreach (var archetype in Archetypes.ToArray())
            {
                if (archetype.Count == 0) DisposeArchetype(archetype);
            }
        }
    }


    private void DisposeArchetype(Archetype archetype)
    {
        Debug.Assert(archetype.IsEmpty, $"{archetype} is not empty?!");
        Debug.Assert(_typeGraph.ContainsKey(archetype.Signature), $"{archetype} is not in type graph?!");
        
        _typeGraph.Remove(archetype.Signature);

        foreach (var type in archetype.Signature)
        {
            // Same here, if all Archetypes with a Type are gone, we can clear the entry.
            _tablesByType[type].Remove(archetype);
            if (_tablesByType[type].Count == 0) _tablesByType.Remove(type);
        }

        foreach (var query in _queries)
        {
            // TODO: Will require some optimization later.
            query.ForgetArchetype(archetype);
        }
        
        //TODO: Maybe make these a virtual property or so.
        Archetypes.Clear();
        Archetypes.AddRange(_typeGraph.Values);        
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
