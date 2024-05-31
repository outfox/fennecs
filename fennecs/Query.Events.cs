namespace fennecs;

public delegate void EntitySpanAction(Span<Entity> obj);

public class QueryEvent
{
    public event EntitySpanAction? Event;
    
    public void Invoke(Span<Entity> entities)
    {
        Event?.Invoke(entities);
    }
}

public partial class Query
{
    private readonly Dictionary<TypeExpression, QueryEvent> _entityEntered = new();
    private readonly Dictionary<TypeExpression, QueryEvent> _entityLeft = new();
    
    public QueryEvent ComponentAdded<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityEntered.TryGetValue(type, out var queryEvent)) return queryEvent;

        queryEvent = new QueryEvent();
        _entityEntered[type] = queryEvent;

        return queryEvent;
    }

    public void Test()
    {
        var query = new Query();

        query.ComponentAdded<int>(Match.Any).Event += entities =>
        {
        };
        
    }
}
