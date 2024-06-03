namespace fennecs;

public readonly record struct Match(TypeExpression value)
{
    public static Match Any<T>() => new(TypeExpression.Of<T>(Target.Any));
    public static Match AnyEntity<T>() => new(TypeExpression.Of<T>(fennecs.Entity.Any));
    public static Match AnyTarget<T>() => new(TypeExpression.Of<T>(Target.AnyTarget));
    public static Match AnyObject<T>() => new(TypeExpression.Of<T>(Target.Object));
    
    public static Match Plain<T>() => new(Component.Plain<T>().value);
    public static Match Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    public static Match Object<T>(T target) where T : class => new(TypeExpression.Of<T>(Link.With(target)));
    
    internal bool Matches(Component other) => value.Matches(other.value);

    internal TypeExpression TypeExpression => value;
    
    // IDEA: This would only be needed for Query globs or something.
    // internal bool Matches(TypeExpression other) => value.Matches(other);
}


internal readonly record struct Component(TypeExpression value)
{
    /// <summary>
    /// <para>
    /// <c>default</c><br/>In Query Matching; matches ONLY Plain Components, i.e. those without a Relation Target.
    /// </para>
    /// <para>
    /// Since it's specific, this Match Expression is always free and has no enumeration cost.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Not a wildcard. Formerly known as "None", as plain components without a target
    /// can only exist once per Entity (same as components with a particular target).
    /// </remarks>
    public static Component Plain<T>() => new(TypeExpression.Of<T>(new(Identity.idPlain)));
    
    public static Component Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    public static Component Object<T>(T target) where T : class => new(TypeExpression.Of<T>(Link.With(target)));

    public void Deconstruct(out TypeExpression output) => output = value;
}
