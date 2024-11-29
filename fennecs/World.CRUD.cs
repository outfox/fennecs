using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Identity = identity, TypeExpression = typeExpression, Data = data});
            return;
        }

        AssertAlive(identity);

        ref var meta = ref _meta[identity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {identity} already has a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill(typeExpression, data, 1);
    }


    internal void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Identity = identity, TypeExpression = typeExpression});
            return;
        }

        ref var meta = ref _meta[identity.Index];

        var oldArchetype = meta.Archetype;

        if (!oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {identity} does not have a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }


    internal bool HasComponent<T>(Identity identity, Key key)
    {
        var type = TypeExpression.Of<T>(key);
        return HasComponent(identity, type);
    }

    
    internal ref T GetComponent<T>(Identity identity, Key key) where T : notnull
    {
        AssertAlive(identity);

        if (!HasComponent<T>(identity, key))
        {
            throw new InvalidOperationException($"Entity {identity} does not have a reference type component of type {typeof(T)} / {key}");
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage<T>(key);
        return ref storage.Span[row];
    }
    
    internal bool GetComponent(Identity identity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        AssertAlive(identity);

        if (!HasComponent(identity, type))
        {
            value = null;
            return false;
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage(type);
        value = storage.Get(row);
        return true;
    }
    
    internal Signature GetSignature(Identity identity)
    {
        AssertAlive(identity);
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }
    #endregion

    internal T[] Get<T>(Identity id, Match match) where T : notnull
    {
        var expression = MatchExpression.Of<T>(match);
        var meta = _meta[id.Index];
        using var storages = meta.Archetype.Match<T>(expression);
        return storages.Select(s => s[meta.Row]).ToArray();
    }
}