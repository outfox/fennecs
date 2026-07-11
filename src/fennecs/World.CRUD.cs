using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs;

public partial class World
{
    #region CRUD

    // These facades perform the World-level concerns (deferred mode, liveness assertions)
    // and delegate the structural work to the Aspect that stores the component type.

    internal void AddComponent<T>(Entity entity, TypeExpression typeExpression, T data) where T : notnull
    {
        // (manual null check: the JIT eliminates the branch entirely for value type T)
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (data == null) ThrowDataNull();

        if (typeExpression.isWildcard) ThrowWildcard("Cannot add a Wildcard Component", nameof(typeExpression));

        // Fails fast at the call site, even in Deferred mode.
        AssertSameWorld(typeExpression);

        // Resolve before deferring: fails fast at the call site under StrictAspects.
        var aspect = AspectOf(typeExpression);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Entity = entity, TypeExpression = typeExpression, Data = data});
            return;
        }

        AssertAlive(entity);

        aspect.AddComponent(entity, typeExpression, data);
    }


    internal void RemoveComponent(Entity entity, TypeExpression typeExpression)
    {
        var aspect = AspectOf(typeExpression);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Entity = entity, TypeExpression = typeExpression});
            return;
        }

        AssertAlive(entity);

        aspect.RemoveComponent(entity, typeExpression);
    }


    internal bool HasComponent<T>(Entity entity, Match match)
    {
        var type = TypeExpression.Of<T>(match);
        return HasComponent(entity, type);
    }


    internal ref T GetComponent<T>(Entity entity, Match match)
    {
        AssertAlive(entity);
        return ref AspectOf(TypeExpression.Of<T>(match)).GetComponent<T>(entity, match);
    }

    internal bool GetComponent(Entity entity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        if (type.isWildcard) ThrowWildcard("Cannot get a Wildcard Component", nameof(type));

        AssertAlive(entity);

        return AspectOf(type).GetComponent(entity, type, out value);
    }

    internal Signature GetSignature(Entity entity)
    {
        AssertAlive(entity);

        var signature = Main.GetSignature(entity);
        for (var i = 1; i < _aspects.Count; i++)
        {
            var aspect = _aspects[i];
            if (!aspect.Contains(entity)) continue;
            signature = signature.Union(aspect.GetSignature(entity));
        }
        return signature;
    }
    #endregion

    internal T[] Get<T>(Entity entity, Match match)
    {
        if (!IsAlive(entity)) return [];
        return AspectOf(TypeExpression.Of<T>(match)).Get<T>(entity, match);
    }


    #region Assert & Throw Helpers

    /// <summary>
    /// Entities are world-relative: relations may not target Entities of another World.
    /// (a single byte compare — the relation Key carries its target's world tag)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AssertSameWorld(TypeExpression typeExpression)
    {
        var key = typeExpression.Key;
        if (key.IsEntity && key.WorldTag != Tag) ThrowForeignRelation(key);
    }


    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowForeignRelation(Key key) =>
        throw new InvalidOperationException(
            $"Relation target {key} belongs to another World — relations cannot target Entities of other Worlds. (this is World \"{Name}\", tag {Tag})");


    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    // ReSharper disable once NotResolvedInText
    private static void ThrowDataNull() => throw new ArgumentNullException("data");


    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWildcard(string message, string paramName) => throw new ArgumentException(message, paramName);

    #endregion
}
