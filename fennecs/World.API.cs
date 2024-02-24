using System.Diagnostics.CodeAnalysis;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    /// <summary>
    /// Creates a new entity in this World.
    /// Reuses previously despawned Entities, who will differ in generation after respawn. 
    /// </summary>
    /// <returns>an EntityBuilder to operate on</returns>
    public Entity Spawn() => new(this, NewEntity());

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
    /// provide a more intuitive verb to "get" an Entity to assign a variable.
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


    #region Linked Components
    
    /* Idea for alternative API
    public struct Linked<T>(T link)
    {
        public readonly T Link = link;

        public static Linked<O> With<O>(O link) where O : class
        {
            return new Linked<O>(link);
        }

        public static Linked<Identity> With(Identity link)
        {
            return new Linked<Identity>(link);
        }
    }

    public void Add<T>(Identity identity, Linked<T> target) where T : class
    {
        var linkIdentity = _referenceStore.Request(target.Link);
        var typeExpression = TypeExpression.Create<T>(linkIdentity);
        AddComponent(identity, typeExpression, target);
    }
    */

    /// <summary>
    /// Creates an Archetype relation between this identity and an object (instance of a class).
    /// The relation is backed by the object itself, which will be enumerated by queries if desired.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public void AddLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        AddComponent(identity, typeExpression, target);
    }
    
    /// <summary>
    /// Checks if this identity has an object-backed relation (instance of a class).
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        return HasComponent(identity, typeExpression);
    }

    /// <summary>
    /// Removes the object-backed relation between this identity and the object (instance of a class).
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public void RemoveLink<T>(Identity identity, T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        RemoveComponent(identity, typeExpression);
    }


    /// <summary>
    /// Creates an Archetype relation between this identity and another identity.
    /// The relation is backed by an arbitrary type to provide additional data.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <typeparam name="T">any Component type</typeparam>
    public void AddRelation<T>(Identity identity, Identity target, T data)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        AddComponent(identity, typeExpression, data);
    }
    
    /// <summary>
    /// Checks if this identity has a relation Component with another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool HasRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        return HasComponent(identity, typeExpression);
    }

    /// <summary>
    /// Removes the relation Component between this identity and another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
    public void RemoveRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        RemoveComponent(identity, typeExpression);
    }

    #endregion
    
    public void AddComponent<T>(Identity identity) where T : new()
    {
        var type = TypeExpression.Create<T>(Match.Plain);
        AddComponent(identity, type, new T());
    }

    
    public void AddComponent<T>(Identity identity, T data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var type = TypeExpression.Create<T>();
        AddComponent(identity, type, data);
    }

    
    public bool HasComponent<T>(Identity identity, Identity target = default)
    {
        var type = TypeExpression.Create<T>(target);
        return HasComponent(identity, type);
    }

    
    public void RemoveComponent<T>(Identity identity)
    {
        var type = TypeExpression.Create<T>(Match.Plain);
        RemoveComponent(identity, type);
    }
    

    
    public void DespawnAllWith<T>(Identity target = default)
    {
        using var query = Query<Identity>().Has<T>(target).Build();
        query.ForSpan(delegate(Span<Identity> entities)
        {
            foreach (var identity in entities) Despawn(identity);
        });
    }
    
    public World(int capacity = 4096)
    {
        _identityPool = new IdentityPool(capacity);
        
        _meta = new Meta[capacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = AddTable([TypeExpression.Create<Identity>(Match.Plain)]);
    }

    public void Despawn(Identity identity)
    {
        lock (_spawnLock)
        {
            AssertAlive(identity);

            if (_mode == Mode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Despawn, Identity = identity});
                return;
            }

            ref var meta = ref _meta[identity.Index];

            var table = meta.Archetype;
            table.Remove(meta.Row);
            meta.Clear();

            _identityPool.Despawn(identity);

            // Find identity-identity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.TryGetValue(identity, out var list)) return;

            //Remove Components from all Entities that had a relation
            foreach (var type in list)
            {
                var tablesWithType = _tablesByType[type];

                //TODO: There should be a bulk remove method instead.
                foreach (var tableWithType in tablesWithType)
                {
                    for (var i = tableWithType.Count - 1; i >= 0; i--)
                    {
                        RemoveComponent(tableWithType.Identities[i], type);
                    }
                }
            }
        }
    }

    private void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data)
    {
        AssertAlive(identity);

        ref var meta = ref _meta[identity.Index];
        var oldTable = meta.Archetype;

        if (oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"Identity {identity} already has component of type {typeExpression}");
        }

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Add, Identity = identity, TypeExpression = typeExpression, Data = data!});
            return;
        }

        var oldEdge = oldTable.GetTableEdge(typeExpression);

        var newTable = oldEdge.Add;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.Add(typeExpression);
            newTable = AddTable(newTypes);
            oldEdge.Add = newTable;

            var newEdge = newTable.GetTableEdge(typeExpression);
            newEdge.Remove = oldTable;
        }

        var newRow = Archetype.MoveEntry(identity, meta.Row, oldTable, newTable);
        newTable.Set(typeExpression, data, newRow);

        meta.Row = newRow;
        meta.Archetype = newTable;
    }

    public ref T GetComponent<T>(Identity identity, Identity target = default)
    {
        AssertAlive(identity);

        if (typeof(T) == typeof(Identity))
        {
            throw new TypeAccessException("Not allowed get mutable reference in root table (TypeExpression<Identity>, system integrity).");
        }

        var meta = _meta[identity.Index];
        var table = meta.Archetype;
        var storage = table.GetStorage<T>(target);
        return ref storage[meta.Row];
    }
}
