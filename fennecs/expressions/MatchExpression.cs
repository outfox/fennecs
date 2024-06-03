namespace fennecs;

internal readonly record struct Match(TypeExpression value)
{
    public static Match Any<T>() => new(TypeExpression.Of<T>(Target.Any));
    public static Match AnyEntity<T>() => new(TypeExpression.Of<T>(fennecs.Entity.Any));
    public static Match AnyTarget<T>() => new(TypeExpression.Of<T>(Target.AnyTarget));
    public static Match AnyObject<T>() => new(TypeExpression.Of<T>(Target.Object));
    public static Match OnlyPlain<T>() => new(TypeExpression.Of<T>(Target.Plain));

    public static Match Plain<T>() => new(TypeExpression.Of<T>(Target.Plain));
    public static Match Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    public static Match Object<T>(T target) where T : class => new(TypeExpression.Of<T>(Link.With(target)));
    
    internal bool Matches(Component other) => value.Matches(other.value);

    // IDEA: This would only be needed for Query globs or something.
    // internal bool Matches(TypeExpression other) => value.Matches(other);
}


internal readonly record struct Component(TypeExpression value)
{
    public static Component Plain<T>() => new(TypeExpression.Of<T>(Target.Plain));
    public static Component Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    public static Component Object<T>(T target) where T : class => new(TypeExpression.Of<T>(Link.With(target)));
}
