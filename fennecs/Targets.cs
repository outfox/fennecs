using System.Diagnostics.CodeAnalysis;

namespace fennecs;

/// <summary>
/// Target Expression to build a relation.
/// </summary>
public readonly record struct Relate
{
    private Identity Value { get; init; }
    
    internal Relate(Identity identity) => Value = identity;
    
    /// <summary>
    /// Create a Relation expression to the Target entity.
    /// </summary>
    public static Relate To(Entity entity) => new(entity.Id);

    /// <summary>
    /// Implicit conversion from Entity to Relation Target.
    /// </summary>
    public static implicit operator Relate(Entity entity) => new(entity.Id);

    /// <summary>
    /// Implicit conversion from Identity to Generic Target.
    /// </summary>
    public static implicit operator Target(Relate self)
    {
        // Unfortunately, plain is not default here, but it helps a lot with the other match
        // Expressions. But actually this. That we have strong ID types.
        return self.Value == default ? Identity.Plain : new(self.Value);
    }
    
    /// <inheritdoc />
    public override string ToString() => Value.ToString();
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
    public static implicit operator Target(Link<T> self)
    {
        return new(Identity.Of(self.Object));
    }
   
    internal void Deconstruct(out T value)
    {
        value = Object;
    }
    
    /// <inheritdoc />
    public override string ToString() => Object?.ToString() ?? "null";
}

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
