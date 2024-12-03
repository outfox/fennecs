using System.Runtime.CompilerServices;

namespace fennecs;

/// <summary>
/// Experimental extension methods for converting objects to various fennecs types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Convenient Means to convert any object to fennecs.Key (using GetType type detection)
    /// </summary>
    [OverloadResolutionPriority(-1)]
    public static Key Key(this object self) => fennecs.Key.Of(self);

    /// <summary>
    /// Convenient Means to convert any object to fennecs.Key (typed via specialization)
    /// </summary>
    [OverloadResolutionPriority(-1)]
    public static Key Key<T>(this T self) where T : class => fennecs.Key.Of(self);
}


/// <summary>
/// Experimental extension methods for IEnumerables of Archetypes.
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Blits (write, fill) a component value of a type to all entities in the given archetypes that have that component.
    /// </summary>
    public static void Fill<C>(this IEnumerable<Archetype> archetypes, Match match, C value) where C : notnull
    {
        if (typeof(C) == typeof(Entity)) throw new ArgumentException("Cannot blit Entities.");
        if (match.IsLink) throw new ArgumentException("Provided Match Expression matches Links. You cannot fill object Link components with a new value, use a Query Batch Addition/Removal instead.");
        foreach (var archetype in archetypes) archetype.Fill(match, value);
    }
}