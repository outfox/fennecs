using System.Diagnostics.CodeAnalysis;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    /// <summary>
    /// Creates a new entity in this World.
    /// Reuses previously despawned entities, who will differ in generation after respawn. 
    /// </summary>
    /// <returns>an EntityBuilder to operate on</returns>
    public EntityBuilder Spawn() => new(this, NewEntity());

    /// <summary>
    /// Schedule operations on the given identity, via fluid API.
    /// </summary>
    /// <example>
    /// <code>world.On(identity).Add(123).Add("string").Remove&lt;int&gt;();</code>
    /// </example>
    /// <remarks>
    /// The operations will be executed when this object is disposed, or the EntityBuilder's Id() method is called.
    /// </remarks>
    /// <param name="entity"></param>
    /// <returns>an EntityBuilder whose methods return itself, to provide a fluid syntax. </returns>
    public EntityBuilder On(Entity entity) => new(this, entity);

    /// <summary>
    /// Checks if the entity is alive (was not despawned).
    /// </summary>
    /// <param name="entity">an Entity</param>
    /// <returns>true if the Entity is Alive, false if it was previously Despawned</returns>
    public bool IsAlive(Entity entity) => entity.IsReal && entity == _meta[entity.Id].Entity;


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
    /// Whenever the identity is enumerated by a query, it will be batched only with other entities
    /// that share the exact relation, in addition to conforming with the other clauses of the query.
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public void Link<T>(Entity entity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Entity.Of(target));
        AddComponent(entity, typeExpression, target);
    }
    
    /// <summary>
    /// Checks if this identity has an object-backed relation (instance of a class).
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasLink<T>(Entity entity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Entity.Of(target));
        return HasComponent(entity, typeExpression);
    }

    /// <summary>
    /// Removes the object-backed relation between this identity and the object (instance of a class).
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public void Unlink<T>(Entity entity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Entity.Of(target));
        RemoveComponent(entity, typeExpression);
    }


    /// <summary>
    /// Creates an Archetype relation between this identity and another identity.
    /// The relation is backed by an arbitrary type to provide additional data.
    /// Whenever the identity is enumerated by a query, it will be batched only with other entities
    /// that share the exact relation, in addition to conforming with the other clauses of the query.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <typeparam name="T">any component type</typeparam>
    public void Link<T>(Entity entity, Entity target, T data)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        AddComponent(entity, typeExpression, data);
    }
    
    /// <summary>
    /// Checks if this identity has a relation component with another identity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any component type</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool HasLink<T>(Entity entity, Entity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        return HasComponent(entity, typeExpression);
    }

    /// <summary>
    /// Removes the relation component between this identity and another identity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any component type</typeparam>
    public void Unlink<T>(Entity entity, Entity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        RemoveComponent(entity, typeExpression);
    }

    #endregion
    
    public void AddComponent<T>(Entity entity) where T : new()
    {
        var type = TypeExpression.Create<T>(Entity.None);
        AddComponent(entity, type, new T());
    }

    
    public void AddComponent<T>(Entity entity, T data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var type = TypeExpression.Create<T>();
        AddComponent(entity, type, data);
    }

    
    public bool HasComponent<T>(Entity entity, Entity target = default)
    {
        var type = TypeExpression.Create<T>(target);
        return HasComponent(entity, type);
    }

    
    public void RemoveComponent<T>(Entity entity)
    {
        var type = TypeExpression.Create<T>(Entity.None);
        RemoveComponent(entity, type);
    }
    

    
    public void DespawnAllWith<T>(Entity target = default)
    {
        using var query = Query<Entity>().Has<T>(target).Build();
        query.ForSpan(delegate(Span<Entity> entities)
        {
            foreach (var identity in entities) Despawn(identity);
        });
    }
    
    public World(int capacity = 4096)
    {
        _identityPool = new IdentityPool(capacity);
        
        _meta = new EntityMeta[capacity];

        //Create the "Entity" Archetype, which is also the root of the Archetype Graph.
        _root = AddTable([TypeExpression.Create<Entity>(Entity.None)]);
    }

    public void Despawn(Entity entity)
    {
        lock (_spawnLock)
        {
            AssertAlive(entity);

            if (_mode == Mode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Despawn, Entity = entity});
                return;
            }

            ref var meta = ref _meta[entity.Id];

            var table = meta.Archetype;
            table.Remove(meta.Row);
            meta.Clear();

            _identityPool.Despawn(entity);

            // Find identity-identity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.TryGetValue(entity, out var list)) return;

            //Remove components from all entities that had a relation
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

    private void AddComponent<T>(Entity entity, TypeExpression typeExpression, T data)
    {
        AssertAlive(entity);

        ref var meta = ref _meta[entity.Id];
        var oldTable = meta.Archetype;

        if (oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"Identity {entity} already has component of type {typeExpression}");
        }

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Add, Entity = entity, TypeExpression = typeExpression, Data = data!});
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

        var newRow = Archetype.MoveEntry(entity, meta.Row, oldTable, newTable);
        newTable.Set(typeExpression, data, newRow);

        meta.Row = newRow;
        meta.Archetype = newTable;
    }

    public ref T GetComponent<T>(Entity entity, Entity target = default)
    {
        AssertAlive(entity);

        if (typeof(T) == typeof(Entity))
        {
            throw new TypeAccessException("Not allowed get mutable reference in root table (TypeExpression<Identity>, system integrity).");
        }

        var meta = _meta[entity.Id];
        var table = meta.Archetype;
        var storage = table.GetStorage<T>(target);
        return ref storage[meta.Row];
    }
}
