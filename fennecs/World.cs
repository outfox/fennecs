// SPDX-License-Identifier: MIT

namespace fennecs;

public sealed class World : IDisposable
{
    //private readonly Entity _world;

    private readonly Archetypes _archetypes = new();

    public int Count => _archetypes.Count;
    
    public EntityBuilder Spawn()
    {
        return new EntityBuilder(this, _archetypes.Spawn());
    }

    
    public EntityBuilder On(Entity entity)
    {
        return new EntityBuilder(this, entity);
    }

    
    public void Despawn(Entity entity)
    {
        _archetypes.Despawn(entity.Identity);
    }

    
    public void DespawnAllWith<T>() where T : struct
    {
        var query = Query<Entity>().Has<T>().Build();
        query.Run(delegate (Span<Entity> entities)
        {
            foreach (var entity in entities) Despawn(entity);
        });
    }

    
    public bool IsAlive(Entity entity)
    {
        return _archetypes.IsAlive(entity.Identity);
    }


    public ref T GetComponent<T>(Entity entity) where T : struct
    {
        return ref _archetypes.GetComponent<T>(entity);
    }

    public ref T GetComponent<T>(Entity entity, Identity target) where T : struct
    {
        return ref _archetypes.GetComponent<T>(entity, target);
    }

    
    public bool HasComponent<T>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(Identity.None);
        return _archetypes.HasComponent(type, entity);
    }

    
    public void AddComponent<T>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(Identity.None);
        _archetypes.AddComponent(type, entity.Identity, new T());
    }

    
    public void AddComponent<T>(Entity entity, T component) {
        var type = TypeExpression.Create<T>(Identity.None);
        _archetypes.AddComponent(type, entity.Identity, component);
    }

    
    public void RemoveComponent<T>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(Identity.None);
        _archetypes.RemoveComponent(type, entity.Identity);
    }

    
    public IEnumerable<(TypeExpression, object)> GetComponents(Entity entity)
    {
        return _archetypes.GetComponents(entity.Identity);
    }

    
    public Ref<T> GetComponent<T>(Entity entity, Entity target) where T : struct
    {
        return new Ref<T>(ref _archetypes.GetComponent<T>(entity.Identity, target.Identity));
    }
        
    
    public bool TryGetComponent<T>(Entity entity, out Ref<T> component) where T : struct
    {
        if (!HasComponent<T>(entity))
        {
            component = default;
            return false;
        }

        component = new Ref<T>(ref _archetypes.GetComponent<T>(entity.Identity, Identity.None));
        return true;
    }

    
    public bool HasComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = TypeExpression.Create<T>(target.Identity);
        return _archetypes.HasComponent(type, entity.Identity);
    }


    public bool HasComponent<T>(Entity entity, Type target) where T : struct
    {
        var type = TypeExpression.Create<T>(new Identity(target));
        return _archetypes.HasComponent(type, entity.Identity);
    }

    /* Todo: probably not worth it
    public bool HasComponent<T, Target>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(new Identity(LanguageType<Target>.Id));
        return _archetypes.HasComponent(type, entity.Identity);
    }
    */


    public void AddComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = TypeExpression.Create<T>(target.Identity);
        _archetypes.AddComponent(type, entity.Identity, new T(), target);
    }


    /* Todo: probably not worth it
    public void AddComponent<T, Target>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T, Target>();
        _archetypes.AddComponent(type, entity.Identity, new T());
    }
    */


    public void AddComponent<T>(Entity entity, T component, Entity target) where T : struct
    {
        var type = TypeExpression.Create<T>(target.Identity);
        _archetypes.AddComponent(type, entity.Identity, component, target);
    }


    public void RemoveComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = TypeExpression.Create<T>(target.Identity);
        _archetypes.RemoveComponent(type, entity.Identity);
    }


    public Entity GetTarget<T>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(Identity.None);
        return _archetypes.GetTarget(type, entity.Identity);
    }


    public IEnumerable<Entity> GetTargets<T>(Entity entity) where T : struct
    {
        var type = TypeExpression.Create<T>(Identity.None);
        return _archetypes.GetTargets(type, entity.Identity);
    }


    [Obsolete("Use new Identity(Type) instead.")]
    internal Entity GetTypeEntity(Type type) => new Identity(type);

    public void Dispose()
    {
    }


    /* I don't think this is necessary.
    public T GetElement<T>() where T : class
    {
        return _archetypes.GetComponent<Element<T>>(_world.Identity, Identity.None).Value;
    }


    public bool TryGetElement<T>(out T? element) where T : class
    {
        if (!HasElement<T>())
        {
            element = null;
            return false;
        }

        element = _archetypes.GetComponent<Element<T>>(_world.Identity, Identity.None).Value;
        return true;
    }


    public bool HasElement<T>() where T : class
    {
        var type = TypeExpression.Create<Element<T>>(Identity.None);
        return _archetypes.HasComponent(type, _world.Identity);
    }


    public void AddElement<T>(T element) where T : class
    {
        var type = TypeExpression.Create<Element<T>>(Identity.None);
        _archetypes.AddComponent(type, _world.Identity, new Element<T> { Value = element });
    }


    public void ReplaceElement<T>(T element) where T : class
    {
        RemoveElement<T>();
        AddElement(element);
    }


    public void AddOrReplaceElement<T>(T element) where T : class
    {
        if (HasElement<T>())
        {
            _archetypes.GetComponent<Element<T>>(_world.Identity, Identity.None).Value = element;
        }

        AddElement(element);
    }


    public void RemoveElement<T>() where T : class
    {
        var type = TypeExpression.Create<Element<T>>(Identity.None);
        _archetypes.RemoveComponent(type, _world.Identity);
    }
     */
    

    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(_archetypes);
    }

    public QueryBuilder<C> Query<C>()
    {
        return new QueryBuilder<C>(_archetypes);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>() where C2 : struct
    {
        return new QueryBuilder<C1, C2>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : struct where C2 : struct where C3 : struct
    {
        return new QueryBuilder<C1, C2, C3>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(_archetypes);
    }

    /*
    public QueryBuilder<C1, C2, C3, C4, C5, C6> Query<C1, C2, C3, C4, C5, C6>() where C1 : struct
        where C2 : struct
        where C3 : struct
        where C4 : struct
        where C5 : struct
        where C6 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5, C6>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Query<C1, C2, C3, C4, C5, C6, C7>() where C1 : struct
        where C2 : struct
        where C3 : struct
        where C4 : struct
        where C5 : struct
        where C6 : struct
        where C7 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5, C6, C7>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Query<C1, C2, C3, C4, C5, C6, C7, C8>()
        where C1 : struct
        where C2 : struct
        where C3 : struct
        where C4 : struct
        where C5 : struct
        where C6 : struct
        where C7 : struct
        where C8 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>(_archetypes);
    }
    */


    /* I don't think this is necessary.
    public void Tick()
    {
        _worldInfo.EntityCount = _archetypes.EntityCount;
        _worldInfo.UnusedEntityCount = _archetypes.UnusedIds.Count;
        _worldInfo.AllocatedEntityCount = _archetypes.Meta.Length;
        _worldInfo.ArchetypeCount = _archetypes.Tables.Count;
        // info.RelationCount = relationCount;
        _worldInfo.ElementCount = _archetypes.Tables[_archetypes.Meta[_world.Identity.Id].TableId].Types.Count;
        _worldInfo.QueryCount = _archetypes.Queries.Count;
    }
    */
}