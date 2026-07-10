// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs;

public partial class Aspect
{
    #region CRUD

    // Deferred-mode checks and liveness asserts live on the World facades (World.CRUD.cs);
    // these cores operate immediately on this Aspect's Archetypes.

    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (!Contains(identity))
        {
            // Lazy membership: the first Component owned by this Aspect joins the Entity
            // directly into the {Identity, T} Archetype. (single insert, no intermediate move)
            EnsureCapacity(identity.Index + 1);
            var signature = new Signature(Comp<Identity>.Plain.Expression).Add(typeExpression);
            var archetype = GetArchetype(signature);
            archetype.JoinWith(identity, typeExpression, data);
            return;
        }

        ref var meta = ref _meta[identity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {identity} already has a Component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill(typeExpression, data, 1);
    }


    internal void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        if (!Contains(identity)) throw new InvalidOperationException($"Entity {identity} does not have a Component of type {typeExpression}");

        ref var meta = ref _meta[identity.Index];

        var oldArchetype = meta.Archetype;

        if (!oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {identity} does not have a Component of type {typeExpression}");

        var newSignature = oldArchetype.Signature.Remove(typeExpression);

        // Lazy membership: removing the last owned Component evicts the Entity from this Aspect.
        // (Main keeps all living Entities, at minimum in its Root archetype)
        if (!IsMain && newSignature.Count == 1)
        {
            oldArchetype.Delete(meta.Row);
            _meta[identity.Index] = default;
            return;
        }

        var newArchetype = GetArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        if (identity.Index >= _meta.Length) return false;

        var meta = _meta[identity.Index];
        return meta.Identity != default
               && meta.Identity == identity
               && typeExpression.Matches(meta.Archetype.MatchSignature);
    }


    internal ref T GetComponent<T>(Identity identity, Match match)
    {
        if (!HasComponent(identity, TypeExpression.Of<T>(match)))
        {
            throw new InvalidOperationException($"Entity {identity} does not have a reference type Component of type {typeof(T)} / {match}");
        }

        var (table, row, _) = _meta[identity.Index];
        var storage = table.GetStorage<T>(match);
        return ref storage.Span[row];
    }


    internal bool GetComponent(Identity identity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
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
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Signature;
        return array;
    }


    internal T[] Get<T>(Identity id, Match match)
    {
        if (!Contains(id)) return [];

        var type = TypeExpression.Of<T>(match);
        var meta = _meta[id.Index];
        using var storages = meta.Archetype.Match<T>(type);
        return storages.Select(s => s[meta.Row]).ToArray();
    }


    internal IReadOnlyList<Component> GetComponents(Identity id)
    {
        if (!Contains(id)) return [];

        var archetype = _meta[id.Index].Archetype;
        return archetype.GetRow(_meta[id.Index].Row);
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
        foreach (var identity in archetype.IdentityStorage.Span)
        {
            _meta[identity.Index] = default;
        }
        archetype.Clear();
    }

    #endregion
}
