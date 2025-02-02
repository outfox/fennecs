using System.Diagnostics.CodeAnalysis;

namespace fennecs;

public partial class World
{
    #region CRUD
    internal void AddComponent<T>(Entity entity, TypeExpression typeExpression, T data, string callerFile = "", int callerLine = 0) where T : notnull
    {
        if (data is null) throw new ArgumentNullException(nameof(data), $"Cannot add a null value for component of type {typeExpression}.");
        
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new() {Opcode = Opcode.Add, Entity = entity, TypeExpression = typeExpression, Data = data, File = callerFile, Line = callerLine});
            return;
        }

        AssertAlive(entity);

        ref var meta = ref _meta[entity.Index];
        var oldArchetype = meta.Archetype;

        if (oldArchetype.Signature.Matches(typeExpression)) throw new InvalidOperationException($"Entity {entity} already has a component of type {typeExpression}.");

        var newSignature = oldArchetype.Signature.Add(typeExpression);
        var newArchetype = GetOrCreateArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);

        // Back-fill the new value
        newArchetype.BackFill(typeExpression, data, 1);
    }


    internal void RemoveComponent(Entity entity, MatchExpression expression)
    {
        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Entity = entity, MatchExpression = expression});
            return;
        }

        ref var meta = ref _meta[entity.Index];

        var oldArchetype = meta.Archetype;

        if (!oldArchetype.Signature.Matches(expression)) throw new InvalidOperationException($"Cannot remove Component {expression} from Entity {entity}, because it does not or no longer has that component. Did you accidentally try to remove it twice?");

        var newSignature = expression.IsWildcard 
            ? new(oldArchetype.Signature.Where(expression.MatchesNot)) 
            : oldArchetype.Signature.Remove(expression.AsTypeExpression());
        var newArchetype = GetOrCreateArchetype(newSignature);
        Archetype.MoveEntry(meta.Row, oldArchetype, newArchetype);
    }

    
    internal bool HasComponent<T>(Entity entity, Key key) => HasComponent(entity, TypeExpression.Of<T>(key));


    internal bool HasComponent<T>(Entity entity, Match match) => HasComponent(entity, MatchExpression.Of<T>(match));

    
    internal bool HasComponent(Entity entity, Type type, Match match) => HasComponent(entity, MatchExpression.Of(type, match));


    internal ref T GetComponent<T>(Entity entity, Key key) where T : notnull
    {
        AssertAlive(entity);

        if (!HasComponent<T>(entity, key))
        {
            throw new InvalidOperationException($"Entity {entity} does not have a component of type {typeof(T)} / {key}");
        }

        var (table, row) = _meta[entity.Index];
        var storage = table.GetStorage<T>(key);
        return ref storage.Span[row];
    }
    
    
    internal bool TryGetComponent(Entity entity, TypeExpression type, [MaybeNullWhen(false)] out object value)
    {
        AssertAlive(entity);

        if (!HasComponent(entity, type))
        {
            value = null;
            return false;
        }

        var (table, row) = _meta[entity.Index];
        var storage = table!.GetStorage(type);
        value = storage.Get(row);
        return true;
    }
    
    internal Signature GetSignature(Entity entity)
    {
        AssertAlive(entity);
        var meta = _meta[entity.Index];
        var array = meta.Archetype!.Signature;
        return array;
    }
    #endregion

    internal T[] Get<T>(Entity id, Match match) where T : notnull
    {
        var meta = _meta[id.Index];
        using var storages = meta.Archetype!.Match<T>(match);
        return storages.Select(s => s[meta.Row]).ToArray();
    }
    
    internal IReadOnlyList<Component> GetComponents(Entity id)
    {
        var archetype = _meta[id.Index].Archetype;
        return archetype!.GetRow(_meta[id.Index].Row);
    }

}