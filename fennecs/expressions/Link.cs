namespace fennecs;


/// <summary>
/// Helper Class to create Link expressions.
/// </summary>
public static class Link
{
    /// <summary>
    /// Create a Link expression to the Target object.
    /// </summary>
    /// <param name="target">the object</param>
    /// <typeparam name="T">type of the object</typeparam>
    public static Link<T> With<T>(T target) where T : class => Link<T>.With(target);
}


/// <summary>
/// Target Expression to build an Object Link.
/// </summary>
public readonly record struct Link<T> where T : class
{
    internal T Target => Object;

    internal TypeExpression TypeExpression => TypeExpression.Of<T>(this);
    
    /// <summary>
    /// The linked Object.
    /// </summary>
    public required T Object { get; init; }

    /// <summary>
    /// Create a Link expression to the Target object.
    /// </summary>
    public static Link<T> With(T target) => new() {Object = target};

    /// <summary>
    /// Implicit conversion from Object to Link.
    /// </summary>
    public static implicit operator Link<T>(T self) => new() {Object = self};

    /// <summary>
    /// Implicit conversion from Link to generic Target.
    /// </summary>
    public static implicit operator Match(Link<T> self)
    {
        return new(Identity.Of(self.Object));
    }
   
    /// <inheritdoc />
    public override string ToString() => $"Link {TypeExpression} -> {Object?.ToString() ?? "null"}";
}
