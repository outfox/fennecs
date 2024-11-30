﻿namespace fennecs.reflection;

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
    public static Entity AddVirtual(this Entity entity, object value, Key key = default)
    {
        entity.World.AddComponent(entity, TypeExpression.Of(value.GetType(), key), value);
        return entity;
    }


    /// <summary>
    /// Returns all components on the entity that are <see cref="Type.IsAssignableTo"/> to the Type Parameter <c>T</c>.
    /// </summary>
    /// <remarks>
    /// The array is empty if there are no matching components.
    /// TODO: This call has room for optimization, and may benefit from match expression support.
    /// </remarks>
    public static T[] GetVirtual<T>(this Entity entity)
    {
        var filtered = entity.Components
            .Where(c => c.Type.IsAssignableTo(typeof(T)))
            .Select(c => c.Box.Value).Cast<T>().ToArray();
        return filtered;
    }


    /// <summary>
    /// Returns true if the entity has any components that are <see cref="Type.IsAssignableTo"/> to the Type Parameter <c>T</c>.
    /// </summary>
    /// <remarks>
    /// TODO: This call has room for optimization, and may benefit from match expression support.
    /// </remarks>
    public static bool HasVirtual<T>(this Entity entity)
    {
        return entity.Components.Any(c => c.Type.IsAssignableTo(typeof(T)));
    }
}