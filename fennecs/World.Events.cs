#if EXPERIMENTAL
namespace fennecs;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public delegate void EntityComponentSpanAction<T>(Span<Entity> entities, Span<T> components);

public abstract class EntityComponentEvent;
public class EntityComponentEvent<T> : EntityComponentEvent
{
    public event EntityComponentSpanAction<T>? Event;

    public void Invoke(Span<Entity> entities, Span<T> components) => Event?.Invoke(entities, components);
}

public class EntityEvent
{
    public event EntitySpanAction? Event;

    public void Invoke(Span<Entity> entities) => Event?.Invoke(entities);

}

public partial class World
{
    private readonly Dictionary<TypeExpression, EntityComponentEvent> _componentAdded = new();
    private readonly Dictionary<TypeExpression, EntityEvent> _componentRemoved = new();
    
    public EntityComponentEvent<T> ComponentAdded<T>(Identity match)
    {
        var type = TypeExpression.Of<T>(match);
        if (!_componentAdded.TryGetValue(type, out var worldEvent))
        {
            _componentAdded[type] = worldEvent = new EntityComponentEvent<T>();
        }
        return (EntityComponentEvent<T>) worldEvent;
    }

    public EntityEvent ComponentRemoved<T>(Identity match = default)
    {
        var type = TypeExpression.Of<T>(match);
        if (_componentRemoved.TryGetValue(type, out var worldEvent)) return worldEvent;

        _componentRemoved[type] = worldEvent = new();

        return worldEvent;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif