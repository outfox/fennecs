namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data)
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

        if (oldArchetype.Signature.Contains(typeExpression)) throw new ArgumentException($"Entity {identity} already has a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        var newRow = Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill<T>(typeExpression, data);

        meta.Row = newRow;
        meta.Archetype = newArchetype;
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

        if (!oldArchetype.Signature.Contains(typeExpression)) throw new ArgumentException($"Entity {identity} does not have a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        var newRow = Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        meta.Row = newRow;
        meta.Archetype = newArchetype;
    }


    internal bool HasComponent<T>(Identity identity, Identity target)
    {
        var type = TypeExpression.Of<T>(target);
        return HasComponent(identity, type);
    }


    internal ref T GetComponent<T>(Identity identity, Identity target)
    {
        AssertAlive(identity);

        if (typeof(T) == typeof(Identity)) throw new TypeAccessException("Not allowed get mutable reference in root table (TypeExpression<Identity>, system integrity).");

        var meta = _meta[identity.Index];
        var table = meta.Archetype;
        var storage = table.GetStorage<T>(target);
        return ref storage.Span[meta.Row];
    }


    internal Signature<TypeExpression> GetSignature(Identity identity)
    {
        AssertAlive(identity);
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }
    #endregion
}