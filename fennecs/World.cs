// SPDX-License-Identifier: MIT

namespace fennecs;

public sealed class World : IDisposable
{
    //private readonly Entity _world;

    internal readonly Archetypes Archetypes = new();

    public int Count => Archetypes.Count;
    
    public EntityBuilder Spawn()
    {
        return new EntityBuilder(this, Archetypes.Spawn());
    }

    
    public EntityBuilder On(Entity entity)
    {
        return new EntityBuilder(this, entity);
    }

    
    public void Despawn(Entity entity)
    {
        Archetypes.Despawn(entity.Identity);
    }

    
    public void DespawnAllWith<T>() 
    {
        var query = Query<Entity>().Has<T>().Build();
        query.Run(delegate (Span<Entity> entities)
        {
            foreach (var entity in entities) Despawn(entity);
        });
    }

    
    public bool IsAlive(Entity entity)
    {
        return Archetypes.IsAlive(entity.Identity);
    }


    public ref T GetComponent<T>(Entity entity) 
    {
        return ref Archetypes.GetComponent<T>(entity);
    }

    public ref T GetComponent<T>(Entity entity, Identity target) 
    {
        return ref Archetypes.GetComponent<T>(entity, target);
    }

    
    public bool HasComponent<T>(Entity entity) 
    {
        var type = TypeExpression.Create<T>(Identity.None);
        return Archetypes.HasComponent(type, entity);
    }

    
    public void AddComponent<T>(Entity entity) where T : new()
    {
        var type = TypeExpression.Create<T>(Identity.None);
        Archetypes.AddComponent(type, entity.Identity, new T());
    }

    
    public void AddComponent<T>(Entity entity, T component) {
        var type = TypeExpression.Create<T>(Identity.None);
        Archetypes.AddComponent(type, entity.Identity, component);
    }

    
    public void RemoveComponent<T>(Entity entity) 
    {
        var type = TypeExpression.Create<T>(Identity.None);
        Archetypes.RemoveComponent(type, entity.Identity);
    }

    
    public IEnumerable<(TypeExpression, object)> GetComponents(Entity entity)
    {
        return Archetypes.GetComponents(entity.Identity);
    }

    
    public Ref<T> GetComponent<T>(Entity entity, Entity target) 
    {
        return new Ref<T>(ref Archetypes.GetComponent<T>(entity.Identity, target.Identity));
    }
        
    
    public bool TryGetComponent<T>(Entity entity, out Ref<T> component) 
    {
        if (!HasComponent<T>(entity))
        {
            component = default;
            return false;
        }

        component = new Ref<T>(ref Archetypes.GetComponent<T>(entity.Identity, Identity.None));
        return true;
    }

    
    public bool HasComponent<T>(Entity entity, Entity target) 
    {
        var type = TypeExpression.Create<T>(target.Identity);
        return Archetypes.HasComponent(type, entity.Identity);
    }


    public bool HasComponent<T>(Entity entity, Type target) 
    {
        var type = TypeExpression.Create<T>(new Identity(target));
        return Archetypes.HasComponent(type, entity.Identity);
    }

    /* Todo: probably not worth it
    public bool HasComponent<T, Target>(Entity entity) 
    {
        var type = TypeExpression.Create<T>(new Identity(LanguageType<Target>.Id));
        return _archetypes.HasComponent(type, entity.Identity);
    }
    */


    public void AddComponent<T>(Entity entity, Entity target) where T : new()
    {
        var type = TypeExpression.Create<T>(target.Identity);
        Archetypes.AddComponent(type, entity.Identity, new T(), target);
    }


    /* Todo: probably not worth it
    public void AddComponent<T, Target>(Entity entity) 
    {
        var type = TypeExpression.Create<T, Target>();
        _archetypes.AddComponent(type, entity.Identity, new T());
    }
    */


    public void AddComponent<T>(Entity entity, T component, Entity target) 
    {
        var type = TypeExpression.Create<T>(target.Identity);
        Archetypes.AddComponent(type, entity.Identity, component, target);
    }


    public void RemoveComponent<T>(Entity entity, Entity target) 
    {
        var type = TypeExpression.Create<T>(target.Identity);
        Archetypes.RemoveComponent(type, entity.Identity);
    }


    public IEnumerable<Entity> GetTargets<T>(Entity entity) 
    {
        var targets = new List<Entity>();
        Archetypes.CollectTargets<T>(targets, entity);
        return targets;
    }


    [Obsolete("Use new Identity(Type) instead.")]
    internal Entity GetTypeEntity(Type type) => new Identity(type);

    public void Dispose()
    {
    }


    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(Archetypes);
    }

    public QueryBuilder<C> Query<C>()
    {
        return new QueryBuilder<C>(Archetypes);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>() where C2 : struct
    {
        return new QueryBuilder<C1, C2>(Archetypes);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : struct where C2 : struct where C3 : struct
    {
        return new QueryBuilder<C1, C2, C3>(Archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4>(Archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(Archetypes);
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