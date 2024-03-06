using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    /// <summary>
    /// Creates an Archetype relation between this identity and an object (instance of a class).
    /// The relation is backed by the object itself, which will be enumerated by queries if desired.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    internal void AddLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        AddComponent(identity, typeExpression, target);
    }


    /// <summary>
    /// Checks if this identity has an object-backed relation (instance of a class).
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal bool HasLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        return HasComponent(identity, typeExpression);
    }


    /// <summary>
    /// Removes the object-backed relation between this identity and the object (instance of a class).
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    internal void RemoveLink<T>(Identity identity, T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));
        RemoveComponent(identity, typeExpression);
    }


    /// <summary>
    /// Creates an Archetype relation between this identity and another identity.
    /// The relation is backed by an arbitrary type to provide additional data.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <typeparam name="T">any Component type</typeparam>
    internal void AddRelation<T>(Identity identity, Identity target, T data)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        AddComponent(identity, typeExpression, data);
    }


    /// <summary>
    /// Checks if this identity has a relation Component with another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal bool HasRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        return HasComponent(identity, typeExpression);
    }


    /// <summary>
    /// Removes the relation Component between this identity and another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
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


    internal void RemoveComponent<T>(Identity identity)
    {
        var type = TypeExpression.Of<T>(Match.Plain);
        RemoveComponent(identity, type);
    }


    private void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data)
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


    private void RemoveComponent(Identity identity, TypeExpression typeExpression)
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