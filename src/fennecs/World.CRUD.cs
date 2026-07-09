using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD

    // These facades perform the World-level concerns (deferred mode, liveness assertions)
    // and delegate the structural work to the Aspect that stores the component type.

    internal void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data) where T : notnull
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        if (typeExpression.isWildcard) throw new ArgumentException("Cannot add a Wildcard Component");

        // Resolve before deferring: fails fast at the call site under StrictAspects.
        var aspect = AspectOf(typeExpression);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Identity = identity, TypeExpression = typeExpression, Data = data});
            return;
        }

        AssertAlive(identity);

        aspect.AddComponent(identity, typeExpression, data);
    }


    internal void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        var aspect = AspectOf(typeExpression);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Identity = identity, TypeExpression = typeExpression});
            return;
        }

        aspect.RemoveComponent(identity, typeExpression);
    }


    internal bool HasComponent<T>(Identity identity, Match match)
    {
        var type = TypeExpression.Of<T>(match);
        return HasComponent(identity, type);
    }


    internal ref T GetComponent<T>(Identity identity, Match match)
    {
        AssertAlive(identity);
        return ref AspectOf(TypeExpression.Of<T>(match)).GetComponent<T>(identity, match);
    }

    internal bool GetComponent(Identity identity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        if (type.isWildcard) throw new ArgumentException("Cannot get a Wildcard Component", nameof(type));

        AssertAlive(identity);

        return AspectOf(type).GetComponent(identity, type, out value);
    }

    internal Signature GetSignature(Identity identity)
    {
        AssertAlive(identity);

        var signature = Main.GetSignature(identity);
        for (var i = 1; i < _aspects.Count; i++)
        {
            var aspect = _aspects[i];
            if (!aspect.Contains(identity)) continue;
            signature = signature.Union(aspect.GetSignature(identity));
        }
        return signature;
    }
    #endregion

    internal T[] Get<T>(Identity id, Match match) => AspectOf(TypeExpression.Of<T>(match)).Get<T>(id, match);
}
