#if EXPERIMENTAL
namespace fennecs;

public delegate void EntityComponentSpanAction<T>(Span<Entity> entities, Span<T> components);


public class EntityComponentEvent<T>
{
    public event EntityComponentSpanAction<T>? Event;

    public void Invoke(Span<Entity> entities, Span<T> components) => Event?.Invoke(entities, components);
}

public partial class World
{
    private readonly Dictionary<TypeExpression, EntityComponentEvent> _entityEntered = new();
    private readonly Dictionary<TypeExpression, EntityComponentEvent> _entityLeft = new();
    
    [Obsolete("Under Construction", true)]
    public EntityComponentEvent<T> ComponentAdded<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityEntered.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityEntered[type] = queryEvent = new();

        return (EntityComponentEvent<T>) queryEvent;
    }

    [Obsolete("Under Construction", true)]
    public EntityComponentEvent<T> ComponentRemoved<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_entityLeft.TryGetValue(type, out var queryEvent)) return queryEvent;

        _entityLeft[type] = queryEvent = new();

        return (EntityComponentEvent<T>) queryEvent;
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
#endif