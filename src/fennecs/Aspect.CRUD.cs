// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs;

public partial class Aspect
{
    #region CRUD

    // Deferred-mode checks and liveness asserts live on the World facades (World.CRUD.cs);
    // these cores operate immediately on this Aspect's Archetypes, trusting the Entity's index.

    internal void AddComponent<T>(Entity entity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (!Contains(entity))
        {
            // Lazy membership: the first Component owned by this Aspect joins the Entity
            // directly into the {Entity, T} Archetype. (single insert, no intermediate move)
            EnsureCapacity((int) entity.Index + 1);
            var signature = new Signature(Comp<Entity>.Plain.Expression).Add(typeExpression);
            var archetype = GetArchetype(signature);
            archetype.JoinWith(entity, typeExpression, data);
            return;
        }

        ref var meta = ref _meta[entity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {entity} already has a Component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill(typeExpression, data, 1);
    }


    internal void RemoveComponent(Entity entity, TypeExpression typeExpression)
    {
        if (!Contains(entity)) throw new InvalidOperationException($"Entity {entity} does not have a Component of type {typeExpression}");

        ref var meta = ref _meta[entity.Index];

        var oldArchetype = meta.Archetype;

        if (!oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {entity} does not have a Component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);

        // Lazy membership: removing the last owned Component evicts the Entity from this Aspect.
        // (Main keeps all living Entities, at minimum in its Root archetype)
        if (!IsMain && newSignature.Count == 1)
        {
            oldArchetype.Delete(meta.Row);
            _meta[entity.Index] = default;
            return;
        }

        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasComponent(Entity entity, TypeExpression typeExpression)
    {
        if (entity.Index >= (uint) _meta.Length) return false;

        var meta = _meta[entity.Index];
        return meta.Archetype is not null
               && typeExpression.Matches(meta.Archetype.MatchSignature);
    }


    internal ref T GetComponent<T>(Entity entity, Match match)
    {
        if (!HasComponent(entity, TypeExpression.Of<T>(match)))
        {
            throw new InvalidOperationException($"Entity {entity} does not have a reference type Component of type {typeof(T)} / {match}");
        }

        var (table, row) = _meta[entity.Index];
        var storage = table.GetStorage<T>(match);
        return ref storage.Span[row];
    }


    internal bool GetComponent(Entity entity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        if (!HasComponent(entity, type))
        {
            value = null;
            return false;
        }

        var (table, row) = _meta[entity.Index];
        var storage = table.GetStorage(type);
        value = storage.Get(row);
        return true;
    }


    internal Signature GetSignature(Entity entity)
    {
        var meta = _meta[entity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }


    internal T[] Get<T>(Entity entity, Match match)
    {
        if (!Contains(entity)) return [];

        var type = TypeExpression.Of<T>(match);
        var meta = _meta[entity.Index];
        using var storages = meta.Archetype.Match<T>(type);
        return storages.Select(s => s[meta.Row]).ToArray();
    }


    internal IReadOnlyList<Component> GetComponents(Entity entity)
    {
        if (!Contains(entity)) return [];

        var archetype = _meta[entity.Index].Archetype;
        return archetype.GetRow(_meta[entity.Index].Row);
    }

    #endregion


    #region Batch Operations

    internal void Commit(Batch operation)
    {
        foreach (var archetype in operation.Archetypes)
        {
            var preAddSignature = archetype.Signature.Except(operation.Removals);
            var destinationSignature = preAddSignature.Union(operation.Additions);

            // Lazy membership: batch-removing all owned Components evicts the Entities from this Aspect.
            if (!IsMain && destinationSignature.Count == 1)
            {
                BulkEvict(archetype);
                continue;
            }

            var destination = GetArchetype(destinationSignature);
            archetype.Migrate(destination, operation.Additions, operation.BackFill, operation.AddMode);
        }
    }


    private void BulkEvict(Archetype archetype)
    {
        foreach (var entity in archetype.EntityStorage.Span)
        {
            _meta[entity.Index] = default;
        }
        archetype.Clear();
    }

    #endregion
}
