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

    /// <summary>
    /// Match Expressiont to match Any Object Link.
    /// </summary>
    public static Match Any => Match.Object;
    
    
    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Key AnyLink = new((ulong) Key.Kind.Link);
    
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
        return new(Key.Of(self.Object));
    }
   
    /// <inheritdoc />
    public override string ToString() => $"Link {TypeExpression} -> {Object?.ToString() ?? "null"}";
}
