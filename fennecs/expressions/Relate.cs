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
    public static implicit operator Match(Relate self)
    {
        // Unfortunately, plain is not default here, but it helps a lot with the other match
        // Expressions. But actually this. That we have strong ID types.
        return self.Value == default ? Identity.Plain : new(self.Value);
    }
    
    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
