using System.Runtime.CompilerServices;

namespace fennecs;

/// <summary>
/// Experimental extension methods for converting objects to various fennecs types.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Convenient Means to convert any object to fennecs.Key (type detection)
    /// </summary>
    [OverloadResolutionPriority(-1)]
    public static Key Key(this object self) => new(self);

    /// <summary>
    /// Convenient Means to convert any object to fennecs.Key (specifically typed)
    /// </summary>
    public static Key Key<T>(this T self) where T : class => new(self);
}