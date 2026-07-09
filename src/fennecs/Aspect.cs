// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace fennecs;

/// <summary>
/// An Aspect is a self-contained collection of Archetypes within a <see cref="fennecs.World"/> — its own
/// contiguously laid-out component memory universe. Every World has at least one Aspect
/// (<see cref="fennecs.World.Main"/>); additional Aspects share the World's identity space, so the same
/// Entity can have components stored in several Aspects, each laid out independently to
/// combat memory fragmentation.
/// </summary>
public sealed partial class Aspect : IEnumerable<Entity>
{
    /// <summary>
    /// The World this Aspect belongs to. All Aspects of a World share its Entities (identity space).
    /// </summary>
    public World World { get; }

    /// <summary>
    /// The name of this Aspect (unique within its World).
    /// </summary>
    public string Name { get; }


    internal Aspect(World world, string name, int initialCapacity)
    {
        World = world;
        Name = name;
        _meta = new Meta[initialCapacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        Root = GetArchetype(new(Comp<Identity>.Plain.Expression));
    }


    #region State & Storage

    private Meta[] _meta;

    // "Identity" Archetype; all Entities that are members of this Aspect.
    internal readonly Archetype Root;

    private readonly List<Archetype> _archetypes = [];

    private readonly HashSet<Query> _queries = [];
    private readonly Dictionary<int, Query> _queryCache = new();

    // The Type Graph that maps Signatures to Archetypes.
    private readonly Dictionary<Signature, Archetype> _typeGraph = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Relate, HashSet<TypeExpression>> _typesByRelationTarget = new();

    #endregion


    #region Ownership

    /// <summary>
    /// Whether this is its World's <see cref="fennecs.World.Main"/> Aspect.
    /// Every living Entity is a member of Main; other Aspects have lazy membership.
    /// </summary>
    public bool IsMain => ReferenceEquals(World.Main, this);


    /// <summary>
    /// Declares that this Aspect owns (stores) the given Component type.
    /// A type belongs to exactly one Aspect per World; unregistered types live in <see cref="fennecs.World.Main"/>.
    /// Ownership freezes once the type is first used anywhere in the World.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the type is owned by another Aspect, or already in use</exception>
    public Aspect Owns<T>() where T : notnull
    {
        World.RegisterOwnership(this, LanguageType<T>.Id, typeof(T));
        return this;
    }


    /// <inheritdoc cref="Owns{T}()"/>
    /// <param name="types">Component types owned by (stored in) this Aspect</param>
    public Aspect Owns(params Type[] types)
    {
        foreach (var type in types) World.RegisterOwnership(this, LanguageType.Identify(type), type);
        return this;
    }

    #endregion


    #region Membership

    /// <summary>
    /// The number of Entities that are members of this Aspect.
    /// </summary>
    public int Count => _archetypes.Sum(archetype => archetype.Count);


    /// <summary>
    /// Is the Entity a member of this Aspect? (i.e. does it have any Component stored here)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Contains(Identity identity) => identity.Index < _meta.Length && _meta[identity.Index].Identity == identity;


    internal void EnsureCapacity(int capacity)
    {
        if (capacity <= _meta.Length) return;
        Array.Resize(ref _meta, (int)BitOperations.RoundUpToPowerOf2((uint)capacity));
    }


    /// <summary>
    /// Adds the Entity to this Aspect, placing it in the Root archetype.
    /// </summary>
    internal void Join(Identity identity)
    {
        _meta[identity.Index] = new(Root, Root.Count, identity);
        Root.IdentityStorage.Append(identity);
        Root.Invalidate();
    }


    /// <summary>
    /// Removes the Entity's data from this Aspect (if it is a member), and cleans up any
    /// Relations targeting it here. (a Relation can target an Entity that never joined this Aspect)
    /// </summary>
    internal void Despawn(Entity entity)
    {
        if (Contains(entity.Id))
        {
            ref var meta = ref _meta[entity.Id.Index];

            var table = meta.Archetype;
            table.Delete(meta.Row);

            DespawnDependencies(entity);

            // Patch Meta
            _meta[entity.Id.Index] = default;
        }
        else
        {
            DespawnDependencies(entity);
        }
    }


    /// <summary>
    /// Cleans up Relations targeting the Entity and clears its Meta.
    /// The Entity's rows must have already been removed from this Aspect's Archetypes.
    /// (used by <see cref="fennecs.World.Recycle"/> for Archetype.Truncate)
    /// </summary>
    internal void Forget(Entity entity)
    {
        DespawnDependencies(entity);
        _meta[entity.Id.Index] = default;
    }


    private void DespawnDependencies(Entity entity)
    {
        // Find identity-identity relation reverse lookup (if applicable)
        if (!_typesByRelationTarget.TryGetValue(Relate.To(entity), out var types)) return;

        // Collect Archetypes that have any of these relations
        var toMigrate = _archetypes.Where(a => a.Signature.Matches(types)).ToList();

        // Do not change the home archetype of the entity (relating to Entities having a relation with themselves)
        var homeArchetype = entity.Id.Index < _meta.Length ? _meta[entity.Id.Index].Archetype : null;

        // And migrate them to a new Archetype without the relation
        foreach (var archetype in toMigrate)
        {
            if (archetype == homeArchetype) continue;
            if (archetype.Count <= 0) continue;

            var signatureWithoutTarget = archetype.Signature.Except(types);

            // Lazy membership: losing their last owned Components evicts the Entities from this Aspect.
            if (!IsMain && signatureWithoutTarget.Count == 1)
            {
                BulkEvict(archetype);
                continue;
            }

            var destination = GetArchetype(signatureWithoutTarget);
            archetype.Migrate(destination);
        }

        // No longer tracking this Entity
        _typesByRelationTarget.Remove(Relate.To(entity));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAlive(Identity identity) => identity == _meta[identity.Index].Identity;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Meta GetEntityMeta(Identity identity)
    {
        return ref _meta[identity.Index];
    }

    #endregion


    #region Archetypes

    internal Archetype GetArchetype(Signature types)
    {
        if (_typeGraph.TryGetValue(types, out var table)) return table;
        table = new(this, types);

        //This could be given to us by the next query update?
        _archetypes.Add(table);

        // TODO: This is a suboptimal lookup (enumerate dictionary)
        // IDEA: Maybe we can keep Queries in a Tree which
        // identifies them just by their Signature root. (?)
        foreach (var query in _queries)
        {
            if (table.Matches(query.Mask))
            {
                query.TrackArchetype(table);
            }
        }

        foreach (var type in types)
        {
            // Ownership of a type freezes once it is materialized anywhere in the World.
            World.MarkMaterialized(type);

            if (!_tablesByType.TryGetValue(type, out var tableList))
            {
                tableList = new(capacity: 16);
                _tablesByType[type] = tableList;
            }

            tableList.Add(table);

            if (!type.isRelation) continue;

            if (!_typesByRelationTarget.TryGetValue(type.Relation, out var typeSet))
            {
                typeSet = [];
                _typesByRelationTarget[type.Relation] = typeSet;
            }

            typeSet.Add(type);
        }

        _typeGraph.Add(types, table);
        return table;
    }


    /// <summary>
    /// Disposes of empty Archetypes in this Aspect. (see <see cref="fennecs.World.GC"/>)
    /// </summary>
    internal void GC()
    {
        foreach (var archetype in _archetypes.ToArray())
        {
            // The Root archetype is structural (entities join into it) and must never be disposed.
            if (archetype == Root) continue;

            if (archetype.IsEmpty) DisposeArchetype(archetype);
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

        _archetypes.Remove(archetype);
    }

    #endregion


    #region Queries

    internal IReadOnlySet<Query> Queries => _queries;

    internal int ArchetypeCount => _archetypes.Count;


    internal Query CompileQuery(Mask mask)
    {
        // Return cached query if available.
        if (_queryCache.TryGetValue(mask.GetHashCode(), out var query)) return query;

        // A Query matches Archetypes of exactly one Aspect; reject types stored elsewhere.
        // (cached queries were already validated when first compiled)
        foreach (var type in mask.HasTypes.Concat(mask.NotTypes).Concat(mask.AnyTypes))
        {
            if (type.TypeId == LanguageType<Identity>.Id) continue;

            var owner = World.AspectOf(type);
            if (owner == this) continue;

            throw new InvalidOperationException(
                $"Query on Aspect \"{Name}\" includes {type}, which is stored in Aspect \"{owner.Name}\". " +
                "A Query can only match Component types of a single Aspect.");
        }

        //TODO: if we operate on the mask itself, modifications to that mask downstream cause issues.
        //The mask should not be modifiable outside of that scope, so there's an upstream bug.
        // var copy = mask.Clone(); <-- even just copying here hides the race condition

        // Create a new query and cache it.
        var matchingTables = new SortedSet<Archetype>(_archetypes.Where(table => table.Matches(mask)));

        var copy = mask.Clone();
        query = new(this, copy, matchingTables);

        _queries.Add(query);
        _queryCache.Add(copy.GetHashCode(), query);
        return query;
    }


    internal void RemoveQuery(Query query)
    {
        _queries.Remove(query);
        _queryCache.Remove(query.Mask.GetHashCode());
    }

    #endregion


    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<Entity> GetEnumerator()
    {
        return _archetypes.SelectMany(archetype => archetype).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion


    /// <inheritdoc />
    public override string ToString() => $"Aspect \"{Name}\" of {World.Name}: {ArchetypeCount} Archetypes, {Count} Entities, {_queries.Count} Queries";
}
