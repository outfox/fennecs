using System.Runtime.CompilerServices;

namespace fennecs;

/// <summary>
/// Experimental extension methods for converting objects to various fennecs types.
/// </summary>
public static class ObjectExtensions
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
