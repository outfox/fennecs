using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HypEcs;

public struct Element<T>
{
    public T Value;
}
    
public sealed class World
{
    static int worldCount;

    readonly Entity _world;
    readonly WorldInfo _worldInfo;

    readonly Archetypes _archetypes = new();

    public WorldInfo Info => _worldInfo;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public World()
    {
        _world = _archetypes.Spawn();
        _worldInfo = new WorldInfo(++worldCount);
        _archetypes.AddComponent(StorageType.Create<WorldInfo>(Identity.None), _world.Identity, _worldInfo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Spawn()
    {
        return new EntityBuilder(this, _archetypes.Spawn());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder On(Entity entity)
    {
        return new EntityBuilder(this, entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Despawn(Entity entity)
    {
        _archetypes.Despawn(entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DespawnAllWith<T>() where T : struct
    {
        var query = Query<Entity>().Has<T>().Build();
        
        query.Run((count, entities) =>
        {
            for (var i = 0; i < count; i++)
            {
                Despawn(entities[i]);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive(Entity entity)
    {
        return _archetypes.IsAlive(entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>(Entity entity) where T : struct
    {
        return ref _archetypes.GetComponent<T>(entity.Identity, Identity.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(Entity entity) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        return _archetypes.HasComponent(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(Entity entity) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        _archetypes.AddComponent(type, entity.Identity, new T());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(Entity entity, T component) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        _archetypes.AddComponent(type, entity.Identity, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent<T>(Entity entity) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        _archetypes.RemoveComponent(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<(StorageType, object)> GetComponents(Entity entity)
    {
        return _archetypes.GetComponents(entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ref<T> GetComponent<T>(Entity entity, Entity target) where T : struct
    {
        return new Ref<T>(ref _archetypes.GetComponent<T>(entity.Identity, target.Identity));
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = StorageType.Create<T>(target.Identity);
        return _archetypes.HasComponent(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = StorageType.Create<T>(target.Identity);
        _archetypes.AddComponent(type, entity.Identity, new T());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(Entity entity, T component, Entity target) where T : struct
    {
        var type = StorageType.Create<T>(target.Identity);
        _archetypes.AddComponent(type, entity.Identity, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent<T>(Entity entity, Entity target) where T : struct
    {
        var type = StorageType.Create<T>(target.Identity);
        _archetypes.RemoveComponent(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity GetTarget<T>(Entity entity) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        return _archetypes.GetTarget(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Entity> GetTargets<T>(Entity entity) where T : struct
    {
        var type = StorageType.Create<T>(Identity.None);
        return _archetypes.GetTargets(type, entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetElement<T>() where T : class
    {
        return _archetypes.GetComponent<Element<T>>(_world.Identity, Identity.None).Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasElement<T>() where T : class
    {
        var type = StorageType.Create<Element<T>>(Identity.None);
        return _archetypes.HasComponent(type, _world.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddElement<T>(T element) where T : class
    {
        var type = StorageType.Create<Element<T>>(Identity.None);
        _archetypes.AddComponent(type, _world.Identity, new Element<T> { Value = element });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReplaceElement<T>(T element) where T : class
    {
        RemoveElement<T>();
        AddElement(element);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOrReplaceElement<T>(T element) where T : class
    {
        if (HasElement<T>())
        {
            _archetypes.GetComponent<Element<T>>(_world.Identity, Identity.None).Value = element;
        }

        AddElement(element);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveElement<T>() where T : class
    {
        var type = StorageType.Create<Element<T>>(Identity.None);
        _archetypes.RemoveComponent(type, _world.Identity);
    }

    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(_archetypes);
    }

    public QueryBuilder<C> Query<C>() where C : struct
    {
        return new QueryBuilder<C>(_archetypes);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>() where C1 : struct where C2 : struct
    {
        return new QueryBuilder<C1, C2>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : struct where C2 : struct where C3 : struct
    {
        return new QueryBuilder<C1, C2, C3>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : struct
        where C2 : struct
        where C3 : struct
        where C4 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4>(_archetypes);
    }

    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : struct
        where C2 : struct
        where C3 : struct
        where C4 : struct
        where C5 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(_archetypes);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity GetTypeEntity(Type type)
    {
        return _archetypes.GetTypeEntity(type);
    }
}

public sealed class WorldInfo
{
    public readonly int WorldId;
    public int EntityCount;
    public int UnusedEntityCount;
    public int AllocatedEntityCount;

    public int ArchetypeCount;

    // public int RelationCount;
    public int ElementCount;
    public int QueryCount;

    public WorldInfo(int id)
    {
        WorldId = id;
    }
}