using System.Diagnostics.CodeAnalysis;

namespace fennecs;

/// <summary>
/// Target Expression to build a relation.
/// </summary>
public readonly record struct Relate
{
    private Identity Value { get; init; }
    
    internal Relate(Identity identity) => Value = identity;
    
    public static Relate To(Entity entity) => new(entity.Id);

    public static implicit operator Relate(Entity entity) => new(entity.Id);

    public static implicit operator MatchOld(Relate self)
    {
        // Unfortunately, plain is not default here, but it helps a lot with the other match
        // Expressions. But actually this. That we have strong ID types.
        if (self.Value == default) return MatchOld.Plain;
        return new(self.Value);
    }
}

/// <summary>
/// Target Expression to build an Object Link.
/// </summary>
public readonly record struct Link<T> where T : class
{
    internal T Target => Object;

    internal TypeExpression TypeExpression => TypeExpression.Of<T>(this);
    
    public required T Object { get; init; }

    public static Link<T> With(T target) => new() {Object = target};

    public static implicit operator Link<T>(T self) => new() {Object = self};

    public static implicit operator MatchOld(Link<T> self)
    {
        return new(Identity.Of(self.Object));
    }
   
    internal void Deconstruct(out T value)
    {
        value = Object;
    }
}

public static class Link
{
    public static Link<T> With<T>(T target) where T : class => Link<T>.With(target);
}
