namespace fennecs;

/// <summary>
/// A Match Expression that can be used to match components in a Query.
/// TODO: Do not instantiate directly, use the static methods instead.
/// </summary>
/// <param name="value">TypeExpresion backing this Match.</param>
public readonly record struct Match(TypeExpression value)
{
    /// <summary>
    /// Matches any component of type <typeparamref name="T"/> regardless of Target
    /// </summary>
    public static Match Any<T>() => new(TypeExpression.Of<T>(Target.Any));

    /// <summary>
    /// Matches any componentof type <typeparamref name="T"/> with a Entity Target (relation)
    /// </summary>
    public static Match AnyEntity<T>() => new(TypeExpression.Of<T>(fennecs.Entity.Any));
    /// <summary>
    ///  Matches any component of type <typeparamref name="T"/> with an Object Target
    /// </summary>
    public static Match AnyObject<T>() => new(TypeExpression.Of<T>(Target.Object));
    /// <summary>
    /// Matches any component of type <typeparamref name="T"/> with any Target (Object or Entity) 
    /// </summary>
    public static Match AnyTarget<T>() => new(TypeExpression.Of<T>(Target.AnyTarget));
    
    /// <summary>
    ///  Matches the Plain Component of type <typeparamref name="T"/>.
    /// </summary>
    public static Match Plain<T>() => new(Component.Plain<T>().value);
    /// <summary>
    /// Matches the Relation of type <typeparamref name="T"/> with the specified Entity Target.
    /// </summary>
    public static Match Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    /// <summary>
    /// Matches the Link of type <typeparamref name="T"/> with the specified Object Target.
    /// </summary>
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
    public static Component Plain<T>() => new(TypeExpression.Of<T>(new(Wildcard.Plain)));
    
    public static Component Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    public static Component Object<T>(T target) where T : class => new(TypeExpression.Of<T>(Link.With(target)));

    public void Deconstruct(out TypeExpression output) => output = value;
}
