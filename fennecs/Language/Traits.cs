namespace fennecs.Language;

internal readonly record struct Traits
{
    /// <summary>
    /// The underlying type of the TypeId.
    /// </summary>
    public required System.Type Type { get; init; }
    
    /// <summary>
    /// Lookup whether the type is a class. 
    /// </summary>
    public required bool IsClass  { get; init; }

    /// <summary>
    /// Lookup whether the type is a class. 
    /// </summary>
    public required bool IsValueType  { get; init; }

    /// <summary>
    /// Lookup whether the type is an unmanaged type 
    /// </summary>
    public required bool IsUnmanaged { get; init; }
    
    /// <summary>
    /// Lookup whether the type is an interface. 
    /// </summary>
    public required bool IsInterface { get; init; }
}