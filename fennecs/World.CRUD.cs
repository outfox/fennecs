using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        AddComponent(identity, typeExpression, target);
    }


    internal bool HasLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        return HasComponent(identity, typeExpression);
    }


    internal void RemoveLink<T>(Identity identity, T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        RemoveComponent(identity, typeExpression);
    }


    internal void AddRelation<T>(Identity identity, Identity target, T data)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        AddComponent(identity, typeExpression, data);
    }


    internal bool HasRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        return HasComponent(identity, typeExpression);
    }


    internal void RemoveRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        RemoveComponent(identity, typeExpression);
    }


    internal void AddComponent<T>(Identity identity) where T : new()
    {
        var type = TypeExpression.Of<T>(Match.Plain);
        AddComponent(identity, type, new T());
    }


    internal void AddComponent<T>(Identity identity, T data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var type = TypeExpression.Of<T>();
        AddComponent(identity, type, data);
    }


    internal bool HasComponent<T>(Identity identity, Identity target = default)
    {
        var type = TypeExpression.Of<T>(target);
        return HasComponent(identity, type);
    }
    

    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data)
    {
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Identity = identity, TypeExpression = typeExpression, Data = data!});
            return;
        }

        AssertAlive(identity);

        ref var meta = ref _meta[identity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Contains(typeExpression)) throw new ArgumentException($"Entity {identity} already has a component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        var newRow = Archetype.MoveEntry(identity, meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.Set(typeExpression, data, newRow);

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
        var newRow = Archetype.MoveEntry(identity, meta.Row, oldArchetype, newArchetype);

        meta.Row = newRow;
        meta.Archetype = newArchetype;
    }


    internal ref T GetComponent<T>(Identity identity, Identity target = default)
    {
        AssertAlive(identity);

        if (typeof(T) == typeof(Identity)) throw new TypeAccessException("Not allowed get mutable reference in root table (TypeExpression<Identity>, system integrity).");

        var meta = _meta[identity.Index];
        var table = meta.Archetype;
        var storage = table.GetStorage<T>(target);
        return ref storage[meta.Row];
    }


    public IEnumerable<TypeExpression> ListComponents(Identity identity)
    {
        AssertAlive(identity);
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }
    #endregion
}