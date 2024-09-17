﻿namespace fennecs;

/// <summary>
/// Target Expression to build a relation.
/// </summary>
internal readonly record struct Relate2
{
    private Identity Value { get; }
    
    internal Relate(Identity identity) => Value = identity;
    
    /// <summary>
    /// Create a Relation expression to the Target entity.
    /// </summary>
    public static Relate To(Entity entity) => new(entity);

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
        return self.Value == default ? Match.Plain : new(self.Value);
    }
    
    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
