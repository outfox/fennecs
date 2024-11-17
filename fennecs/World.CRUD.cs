﻿using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        
        if (typeExpression.isWildcard) throw new ArgumentException("Cannot add a wildcard component");

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


    internal bool HasComponent<T>(Identity identity, Match match)
    {
        var type = TypeExpression.Of<T>(match);
        return HasComponent(identity, type);
    }

    /* This is sad but can't be done syntactically at the moment (without bloating the interface)
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
    
    internal ref T GetComponent<T>(Identity identity, Match match) where T : notnull
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
    
    internal bool GetComponent(Identity identity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        if (type.isWildcard) throw new ArgumentException("Cannot get a wildcard component", nameof(type));
        
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
        var type = TypeExpression.Of<T>(match);
        var meta = _meta[id.Index];
        using var storages = meta.Archetype.Match<T>(type);
        return storages.Select(s => s[meta.Row]).ToArray();
    }
}