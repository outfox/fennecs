using System.Diagnostics.CodeAnalysis;

namespace fennecs;

/// <summary>
/// Target Expression to build a relation.
/// </summary>
public readonly record struct Relation
{
    private Identity Value { get; init; }
    
    private Relation(Identity identity) => Value = identity;

    public static Relation To(Entity entity) => new(entity.Id);

    public static implicit operator Relation(Entity entity) => new(entity.Id);

    public static implicit operator Match(Relation self)
    {
        // Unfortunately, plain is not default here, but it helps a lot with the other match
        // Expressions. But actually this. That we have strong ID types.
        if (self.Value == default) return Match.Plain;
        return new(self.Value);
    }
}

/// <summary>
/// Target Expression to build an Object Link.
/// </summary>
public readonly record struct Link<T> where T : class
{
    /// <summary>
    /// Target Expression to build an Object Link.
    /// </summary>
    [SetsRequiredMembers]
    public Link(T Object)
    {
        this.Object = Object;
    }
    
    public required T Object { get; init; }
    
    public static Link<T> With(T target) => new(target);

    public static implicit operator Link<T>(T self) => new(self);

    public static implicit operator Match(Link<T> self)
    {
        return new(Identity.Of(self.Object));
    }

    public static implicit operator TypeExpression(Link<T> self)
    {
        return TypeExpression.Of<T>(self);
    }

    internal T Target => Object;
    internal void Deconstruct(out T value)
    {
        value = Object;
    }
}

public static class Link
{
    public static Link<T> With<T>(T target) where T : class => Link<T>.With(target);
}
