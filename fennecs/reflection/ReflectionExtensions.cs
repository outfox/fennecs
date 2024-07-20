namespace fennecs.reflection;

/// <summary>
/// Extension Methods that use some sort of Reflection under the hood.
/// </summary>
/// <summary>
/// These are generally against fennecs design principles, but they do have their use cases.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Experimental method to add a specific component identified via RTTI (dynamically retrieved at runtime).
    /// This helps with contravariant and covariant component types, such as Lists.
    /// Only this call uses the dynamic logic, the component itself is as any normal Component type.
    /// </summary>
    /// <remarks>
    /// This will attempt to create a component type of exactly the object's <see cref="object.GetType"/> returned <c>System.Type</c>.
    /// Note that <c>QueryBuilders</c> will need to use the specific type to match the Component! (e.g. <c>Query&lt;List&lt;int&gt;&gt;</c>)
    /// </remarks>
    public static Entity AddVirtual(this Entity entity, object value, Match match = default)
    {
        entity._world.AddComponent(entity.Id, TypeExpression.Of(value.GetType(), match), value);
        return entity;
    }


    /// <summary>
    /// Returns all components on the entity that are <see cref="Type.IsAssignableTo"/> to the Type Parameter <c>T</c>.
    /// </summary>
    /// <remarks>
    /// The array is empty if there are no matching components.
    /// </remarks>
    public static T[] GetVirtual<T>(this Entity entity)
    {
        var components = entity.Components;
        var filtered = components.Where(c => c.Type.IsAssignableTo(typeof(T))).Select(c => c.Box.Value).Cast<T>().ToArray();
        return filtered;
    }
}