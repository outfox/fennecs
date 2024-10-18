﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace fennecs;

/// <summary>
/// A fennecs.World contains Entities, their Components, compiled Queries, and manages the lifecycles of these objects.
/// </summary>
public partial class World : IDisposable
{
    /// <summary>
    /// Index (unique numeric id) of this world.
    /// </summary>
    /// <remarks>
    /// There can be up to <see cref="MaxWorlds"/> distinct World instances in a single application domain.
    /// </remarks>
    public readonly byte Index;

    /// <summary>
    /// All Worlds that can exist in this application domain.
    /// </summary>
    /// <remarks>
    /// Entries in this array may be null.
    /// </remarks>
    internal static readonly World[] All = new World[MaxWorlds];

    private const int MaxWorlds = 256;
    private static readonly ConcurrentQueue<byte> Available = new(Enumerable.Range(0, MaxWorlds).Select(i => (byte)i));


    #region Config

    /// <summary>
    /// Optional name for the World.
    /// </summary>
    public string Name { get; init; } = "";

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
    public Entity Spawn() => NewEntity();


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
        var signature = new Signature(components.ToImmutableSortedSet()).Add(Comp<Entity>.Plain.Expression);
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
    /// <param name="entity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    internal bool IsAlive(Entity entity)
    {
        return entity.Index >= 0
               && entity.Index < _meta.Length
               && entity == _meta[entity.Index].Entity;
    }



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
        var query = Query<Entity>().Has<T>(match).Stream();
        query.Raw(delegate(Memory<Entity> entities)
        {
            //TODO: This is not good. Need to untangle the types here.
            foreach (var entity in entities.Span) DespawnImpl(entity);
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
            //Deleting backwards is usually faster when deleting one-by one (saves a memcpy for each)
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
    internal void Recycle(ReadOnlySpan<Entity> identities)
    {
        lock (_spawnLock)
        {
            foreach (var entity in identities)
            {
                Debug.Assert(entity.Alive, "Recycling a dead Entity!");
                DespawnDependencies(entity);
                _meta[entity.Index] = default;
            }
            _identityPool.Recycle(identities);
        }
    }
    #endregion


    #region Lifecycle & Locking

    /// <summary>
    /// Retrieve a World by its Id.
    /// </summary>
    /// <param name="id">index of the world</param>
    /// <param name="world">the world, if found, or null</param>
    /// <returns>true if the world exists</returns>
    public static bool TryGet(byte id, [MaybeNullWhen(false)] out World world)
    {
        world = All[id];
        return world != null!;
    }
    
    
    /// <summary>
    /// Create a new World.
    /// </summary>
    /// <param name="initialCapacity">initial Entity capacity to reserve. The world will grow automatically.</param>
    public World(int initialCapacity = 4096)
    {
        if (!Available.TryDequeue(out Index)) throw new InvalidOperationException("No more Worlds available.");
        All[Index] = this;
        
        Name = nameof(World);
        
        // World is also a Query of itself.
        World = this;
       
        _identityPool = new(Index, initialCapacity);

        _meta = new Meta[initialCapacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = GetArchetype(new(Comp<Entity>.Plain.Expression));
    }

    
    /// <summary>
    /// Create a new World.
    /// </summary>
    /// <param name="initialCapacity">initial Entity capacity to reserve. The world will grow automatically.</param>
    public World(byte id, int initialCapacity = 4096)
    {
        if (Available.Contains(id)) throw new InvalidOperationException($"World Id already in use by {All[Index]}");
        
        if (!Available.TryDequeue(out Index)) throw new InvalidOperationException("No more Worlds available.");
        All[Index] = this;
        
        // World is also a Query of itself.
        World = this;
       
        _identityPool = new(Index, initialCapacity);

        _meta = new Meta[initialCapacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = GetArchetype(new(Comp<Entity>.Plain.Expression));
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


    internal void DisposeArchetype(Archetype archetype)
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
        
        Archetypes.Remove(archetype);
    }


    /// <summary>
    /// Disposes of the World. Currently, a no-op.
    /// </summary>
    public new void Dispose()
    {
        //TODO: Dispose all Entities, Object Links, Queries, etc!
        
        All[Index] = null!;
        Available.Enqueue(Index);
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
        return string.IsNullOrEmpty(Name) ? $"World {Index}" : $"World {Index} ({Name})";
    }

    /// <inheritdoc cref="ToString"/>
    public string DebugString()
    {
        var sb = new StringBuilder(ToString());
        sb.AppendLine();
        sb.AppendLine($" {Archetypes.Count} Archetypes");
        sb.AppendLine($" {Count} Entities");
        sb.AppendLine($" {_queries.Count} Queries");
        sb.AppendLine($"{nameof(WorldMode)}.{Mode}");
        return sb.ToString();
    }

    #endregion

}
