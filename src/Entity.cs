using System;
using System.Runtime.CompilerServices;

namespace HypEcs;

public readonly struct Entity
{
    public static readonly Entity None = new(Identity.None);
    public static readonly Entity Any = new(Identity.Any);

    public bool IsAny => Identity == Identity.Any;
    public bool IsNone => Identity == Identity.None;

    public Identity Identity { get; }

    public Entity(Identity identity)
    {
        Identity = identity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Identity.Equals(entity.Identity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return Identity.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return Identity.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Entity left, Entity right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
}

public readonly struct EntityBuilder
{
    internal readonly World World;
    readonly Entity _entity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder(World world, Entity entity)
    {
        World = world;
        _entity = entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Add<T>(Entity target = default) where T : struct
    {
        World.AddComponent<T>(_entity, target);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Add<T>(Type type) where T : struct
    {
        var typeEntity = World.GetTypeEntity(type);
        World.AddComponent<T>(_entity, typeEntity);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Add<T>(T data) where T : struct
    {
        World.AddComponent(_entity, data);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Add<T>(T data, Entity target) where T : struct
    {
        World.AddComponent(_entity, data, target);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Add<T>(T data, Type type) where T : struct
    {
        var typeEntity = World.GetTypeEntity(type);
        World.AddComponent(_entity, data, typeEntity);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Remove<T>() where T : struct
    {
        World.RemoveComponent<T>(_entity);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Remove<T>(Entity target) where T : struct
    {
        World.RemoveComponent<T>(_entity, target);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder Remove<T>(Type type) where T : struct
    {
        var typeEntity = World.GetTypeEntity(type);
        World.RemoveComponent<T>(_entity, typeEntity);
        return this;
    }

    public Entity Id()
    {
        return _entity;
    }
}