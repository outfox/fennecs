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

        if (oldArchetype.Signature.Matches(typeExpression)) throw new ArgumentException($"Entity {identity} already has a component of type {typeExpression}");

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

        if (!oldArchetype.Signature.Matches(typeExpression)) throw new ArgumentException($"Entity {identity} does not have a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }


    internal bool HasComponent<T>(Identity identity, Match match)
    {
        var type = TypeExpression.Of<T>(match);
        return HasComponent(identity, type);
    }

    /*
    internal ref T GetOrCreateComponent<T>(Identity identity, Match match) where T : notnull, new()
    {
        AssertAlive(identity);

        if (!HasComponent<T>(identity, match))
        {
            if (Mode != WorldMode.Immediate) throw new InvalidOperationException("Cannot create bew mutable reference to component in deferred mode. (the Entity did must already have the component)");
            AddComponent<T>(identity, TypeExpression.Of<T>(match), new());
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage<T>(match);
        return ref storage.Span[row];
    }
    */
    
    internal ref T GetComponent<T>(Identity identity, Match match)
    {
        AssertAlive(identity);

        if (!HasComponent<T>(identity, match))
        {
            throw new InvalidOperationException($"Entity {identity} does not have a reference type component of type {typeof(T)} / {match}");
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage<T>(match);
        return ref storage.Span[row];
    }
    
/*
    internal T GetComponent<T>(Identity identity, Match match) where T : class
    {
        AssertAlive(identity);

        if (!HasComponent<T>(identity, match))
        {
           throw new InvalidOperationException($"Entity {identity} does not have a reference type component of type {typeof(T)}");
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage<T>(match);
        return storage.Span[row];
    }
*/

    internal Signature GetSignature(Identity identity)
    {
        AssertAlive(identity);
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }
    #endregion
}