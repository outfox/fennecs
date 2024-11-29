#if EXPERIMENTAL
namespace fennecs;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public delegate void ArchetypeAction(Archetype archetype);

public class QueryEntityEvent
{
    public event EntitySpanAction? Event;
    public void Invoke(Span<Entity> entities) => Event?.Invoke(entities);
}

public partial class Query
{
    private event ArchetypeAction ArchetypeTracked;
    private event ArchetypeAction ArchetypeForgotten;

    private readonly Dictionary<TypeExpression, QueryEntityEvent> _entityEntered = new();
    private readonly Dictionary<TypeExpression, QueryEntityEvent> _entityLeft = new();
    
    public QueryEntityEvent EntitiesEntered<T>(Match match = default)
    {
        var type = TypeExpression.Of<T>(Match);
        if (_entityEntered.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityEntered[type] = queryEvent = new();

        return queryEvent;
    }

    public QueryEntityEvent EntitiesExited<T>(Entity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityLeft.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityLeft[type] = queryEvent = new();

        return queryEvent;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif