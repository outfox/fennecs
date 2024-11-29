using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddComponent<T>(Entity entity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Entity = entity, TypeExpression = typeExpression, Data = data});
            return;
        }

        AssertAlive(entity);

        ref var meta = ref _meta[entity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {entity} already has a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill(typeExpression, data, 1);
    }


    internal void RemoveComponent(Entity entity, TypeExpression typeExpression)
    {
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Entity = entity, TypeExpression = typeExpression});
            return;
        }

        ref var meta = ref _meta[entity.Index];

        var oldArchetype = meta.Archetype;

        if (!oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {entity} does not have a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }


    internal bool HasComponent<T>(Entity entity, Key key)
    {
        var type = TypeExpression.Of<T>(key);
        return HasComponent(entity, type);
    }

    
    internal ref T GetComponent<T>(Entity entity, Key key) where T : notnull
    {
        AssertAlive(entity);

        if (!HasComponent<T>(entity, key))
        {
            throw new InvalidOperationException($"Entity {entity} does not have a reference type component of type {typeof(T)} / {key}");
        }

        var (table, row, _) = _meta[entity.Index];
        var storage = table.GetStorage<T>(key);
        return ref storage.Span[row];
    }
    
    internal bool GetComponent(Entity entity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        AssertAlive(entity);

        if (!HasComponent(entity, type))
        {
            value = null;
            return false;
        }

        var (table, row, _) = _meta[entity.Index];
        var storage = table.GetStorage(type);
        value = storage.Get(row);
        return true;
    }
    
    internal Signature GetSignature(Entity entity)
    {
        AssertAlive(entity);
        var meta = _meta[entity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }
    #endregion

    internal T[] Get<T>(Entity id, Match match) where T : notnull
    {
        var expression = MatchExpression.Of<T>(match);
        var meta = _meta[id.Index];
        using var storages = meta.Archetype.Match<T>(expression);
        return storages.Select(s => s[meta.Row]).ToArray();
    }
}