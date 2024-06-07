namespace fennecs;

/// <summary>
/// A Match Expression that can be used to match components in a Query.
/// TODO: Do not instantiate directly, use the static methods instead.
/// </summary>
public readonly record struct Match
{
    private Match(Identity value)
    {
        this.value = value;
    }

    /// <summary>TypeExpresion backing this Match.</summary>
    internal Identity value { get; }

    /// <summary>
    /// Matches any component, regardless target.
    /// </summary>
    public static Match Any => new(Wildcard.Any);

    /// <summary>
    ///  Matches any component with a target of any type.
    /// </summary>
    public static Match Target => new(Wildcard.Target);

    /// <summary>
    ///  Matches any component with a Entity target.
    ///  </summary>
    public static Match Entity => new(Wildcard.Entity);

    /// <summary>
    ///  Matches any component with a Object Link target.
    ///  </summary>
    public static Match Link => new(Wildcard.Object);

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
    public static Match Plain() => new(Wildcard.Plain);

}

/// <summary>
/// A specific set of match expressions to match specific component types where type parameters are not available.
/// </summary>
public readonly record struct Component
{
    private Component(TypeExpression value)
    {
        this.value = value;
    }
    
    internal TypeExpression value { get; }

    public static Component Any<T>() => new(TypeExpression.Of<T>(new(Wildcard.Any)));
    
    public static Component AnyTarget<T>() => new(TypeExpression.Of<T>(new(Wildcard.Target)));
    
    public static Component AnyEntity<T>() => new(TypeExpression.Of<T>(new(Wildcard.Entity)));
    
    public static Component AnyObject<T>() => new(TypeExpression.Of<T>(new(Wildcard.Object)));
    
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

    /// <summary>
    /// Matches any component backing an Entity-Entity relation.
    /// </summary>
    public static Component Entity<T>(Entity target) => new(TypeExpression.Of<T>(target));

    /// <summary>
    /// Matches the Link of type <typeparamref name="T"/> with the specified Object Target.
    /// </summary>
    public static Component Link<T>(T target) where T : class => new(TypeExpression.Of<T>(fennecs.Link.With(target)));

    internal bool Matches(TypeExpression other) => value.Matches(other);
    
    internal bool Matches(Component other) => value.Matches(other.value);

}
