namespace fennecs;

public delegate void EntitySpanAction(Span<Entity> entities);
public delegate void ArchetypeAction(Archetype archetype);

public class QueryEntityEvent
{
    public event EntitySpanAction? Event;

    public void Invoke(Span<Entity> entities) => Event?.Invoke(entities);
}

public partial class Query
{
    [Obsolete("Under Construction", true)]
    private event ArchetypeAction ArchetypeTracked;
    [Obsolete("Under Construction", true)]
    private event ArchetypeAction ArchetypeForgotten;

    private readonly Dictionary<TypeExpression, QueryEntityEvent> _entityEntered = new();
    private readonly Dictionary<TypeExpression, QueryEntityEvent> _entityLeft = new();
    
    [Obsolete("Under Construction", true)]
    public QueryEntityEvent EntitiesEntered<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityEntered.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityEntered[type] = queryEvent = new();

        return queryEvent;
    }

    [Obsolete("Under Construction", true)]
    public QueryEntityEvent EntitiesExited<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityLeft.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityLeft[type] = queryEvent = new();

        return queryEvent;
    }

    /*
    private void Test()
    {
        var query = new Query();

        query.EntitiesEntered<int>(Match.Any).Event += entities => { };
        ArchetypeForgotten += archetype => { };
    }
    */
}
