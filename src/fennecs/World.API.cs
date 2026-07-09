using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using fennecs.pools;

namespace fennecs;

/// <summary>
/// A fennecs.World contains Entities, their Components, compiled Queries, and manages the lifecycles of these objects.
/// </summary>
public partial class World : IDisposable, IEnumerable<Entity>, IAspect
{
    /// <summary>
    /// The World this IAspect surface belongs to: itself.
    /// (Worlds delegate their query surface to their <see cref="Main"/> Aspect,
    /// resolving other Aspects from the queried Component types.)
    /// </summary>
    World IAspect.World => this;


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
    #region Query
    
    /// <summary>
    /// Universal Query, matching all Entities in the World.
    /// </summary>
    public Query All
    {
        get
        {
            using var mask = MaskPool.Rent();
            mask.Has(TypeExpression.Of<Identity>(Match.Plain));
            return CompileQuery(mask);
        }
    }

    #endregion
    
    #region Entity Spawn, Liveness, and Despawn

    /// <summary>
    /// Creates a new Identity in this World, and returns its Entity builder struct.
    /// Reuses previously despawned Entities, whose Identities will differ in Generation after respawn. 
    /// </summary>
    /// <returns>an Entity to operate on</returns>
    public Entity Spawn() => new(this, NewEntity()); //TODO: Check if semantically legal to spawn in Deferred mode.


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
    /// <param name="count">number of Entities to spawn</param>
    /// <param name="values">Component values</param>
    internal void Spawn(int count, IReadOnlyList<TypeExpression> components, IReadOnlyList<object> values)
    {
        if (_aspects.Count == 1)
        {
            var signature = new Signature(components.ToImmutableSortedSet()).Add(Comp<Identity>.Plain.Expression);
            var archetype = Main.GetArchetype(signature);
            archetype.Spawn(count, components, values);
            return;
        }

        using var worldLock = Lock();
        using var identities = SpawnBare(count);

        // Group the configured components by their owning Aspect.
        var groups = new Dictionary<Aspect, (List<TypeExpression> components, List<object> values)>();
        for (var i = 0; i < components.Count; i++)
        {
            var owner = AspectOf(components[i]);
            if (!groups.TryGetValue(owner, out var group))
            {
                group = ([], []);
                groups[owner] = group;
            }
            group.components.Add(components[i]);
            group.values.Add(values[i]);
        }

        // Main always receives the Entities, at minimum into its Root archetype.
        if (!groups.ContainsKey(Main)) groups[Main] = ([], []);

        foreach (var aspect in _aspects)
        {
            if (!groups.TryGetValue(aspect, out var group)) continue;

            var signature = new Signature(group.components.ToImmutableSortedSet()).Add(Comp<Identity>.Plain.Expression);
            aspect.EnsureCapacity(_identityPool.Created + 1);
            aspect.GetArchetype(signature).SpawnWith(identities, group.components, group.values);
        }
    }

    /// <summary>
    /// Despawn (destroy) an Entity from this World.
    /// </summary>
    /// <param name="entity">the Entity to despawn.</param>
    public void Despawn(Entity entity) => DespawnImpl(entity);

    
    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="identity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAlive(Identity identity) => Main.IsAlive(identity);


    /// <summary>
    /// The number of living Entities in the World.
    /// </summary>
    public int Count => _identityPool.Count;

    /// <summary>
    /// All Queries that exist in this World.
    /// </summary>
    public IReadOnlySet<Query> Queries
    {
        get
        {
            if (_aspects.Count == 1) return Main.Queries;

            var queries = new HashSet<Query>();
            foreach (var aspect in _aspects) queries.UnionWith(aspect.Queries);
            return queries;
        }
    }

    #endregion


    #region Bulk Operations

    /// <summary>
    /// Despawn (destroy) all Entities matching a given Type and Match Expression.
    /// </summary>
    /// <typeparam name="T">any Component type</typeparam>
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
    /// <param name="toDelete">the Entities to despawn (remove)</param>
    internal void Despawn(ReadOnlySpan<Entity> toDelete)
    {
        //Deleting backwards is usually faster when deleting one-by one (saves a memcpy for each)
        for (var i = toDelete.Length - 1; i >= 0; i--)
        {
            DespawnImpl(toDelete[i]);
        }
    }

    /// <summary>
    /// Bulk Recycle Entities from a World.
    /// </summary>
    /// <remarks>
    /// MUST BE REMOVED FROM THE SOURCE ARCHETYPE'S STORAGE! (used by Archetype.Truncate)
    /// Other Aspects still evict the Entities' rows normally.
    /// </remarks>
    /// <param name="source">the Archetype the Entities were truncated from</param>
    /// <param name="identities">the entities to despawn (remove)</param>
    internal void Recycle(Archetype source, ReadOnlySpan<Identity> identities)
    {
        foreach (var identity in identities)
        {
            var entity = new Entity(this, identity);
            foreach (var aspect in _aspects)
            {
                // The source Archetype already removed its own rows.
                if (aspect == source.Aspect) aspect.Forget(entity);
                else aspect.Despawn(entity);
            }
        }
        _identityPool.Recycle(identities);
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

        _initialCapacity = initialCapacity;
        _identityPool = new(initialCapacity);

        Main = new(this, "main", initialCapacity);
        _aspects.Add(Main);
    }


    /// <summary>
    ///  Runs the World's Garbage Collection (placeholder for future GC - currently removes all empty Archetypes).
    /// </summary>
    public void GC()
    {
        if (Mode != WorldMode.Immediate) throw new InvalidOperationException("Cannot run GC while in Deferred mode.");

        foreach (var aspect in _aspects) aspect.GC();
    }


    /// <summary>
    /// Disposes of the World. Currently, a no-op.
    /// </summary>
    public void Dispose()
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
    
    #region IEnumerable
    
    /// <inheritdoc />
    public IEnumerator<Entity> GetEnumerator()
    {
        return Main.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Debug Tools

    /// <inheritdoc />
    public override string ToString()
    {
        return DebugString();
    }
    
    /// <inheritdoc cref="ToString"/>
    public string DebugString()
    {
        var sb = new StringBuilder("World:");
        sb.AppendLine();
        sb.AppendLine($" {_aspects.Sum(aspect => aspect.ArchetypeCount)} Archetypes");
        sb.AppendLine($" {Count} Entities");
        sb.AppendLine($" {_aspects.Sum(aspect => aspect.Queries.Count)} Queries");
        sb.AppendLine($"{nameof(WorldMode)}.{Mode}");
        return sb.ToString();
    }

    #endregion
}
