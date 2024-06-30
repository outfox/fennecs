namespace fennecs;

/// <summary>
/// A boxed Component Expression with its accompanying Type and Value.
/// </summary>
public readonly record struct Component
{
    /// <summary>
    /// The backing Type of this Component.
    /// </summary>
    public Type Type => TypeExpression.Type;

    /// <summary>
    /// The boxed Value of this Component. This is always assignable to the backing Type.
    /// </summary>
    public object Value { get; init; }

    internal Component(TypeExpression typeExpression, object value)
    {
        TypeExpression = typeExpression;
        Value = value;
    }
    
    internal TypeExpression TypeExpression { get; }

    internal bool Matches(Component other) => TypeExpression.Matches(other.TypeExpression);

    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with or without a Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Any)")]
    public static Comp<T> AnyAny<T>() => new(Match.Any);

    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with any (but not no) Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Target)")]
    public static Comp<T> AnyRelation<T>() => new(Match.Target);
    
    /// <summary>
    /// Wildcard for a specific component type, with any Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Entity.Any)")]
    public static Comp<T> AnyEntity<T>() => new(Match.Entity);

    /// <summary>
    /// Strongly-Typed for a specific component type, with any Object Link. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.Any)")]
    public static Comp<T> AnyObject<T>() => new(Link.Any); //new(Match.Object);

    /// <summary>
    /// Strongly-Typed for a specific component type, with no Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Plain")]
    public static Comp<T> PlainComponent<T>() => Comp<T>.Plain;
    
    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(target)")]
    public static Comp<T> SpecificEntity<T>(Entity target) => Comp<T>.Matching(target);

    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Object Link Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.With(target))")]
    public static Comp<T> SpecificLink<T>(T target) where T : class => Comp<T>.Matching(Link.With(target));
}


/// <summary>
/// Typeless (dynamic) Component Expression.
/// This is created by <see cref="Comp{T}"/>.
/// </summary>
/// <remarks>
/// Consider using the factory methods:
/// <ul>
/// <li><see cref="Comp{T}.Plain"/></li>
/// <li><see cref="Comp{T}.Matching"/></li>
/// </ul>
/// </remarks>
public readonly record struct Comp
{
    internal readonly TypeExpression Expression;
    
    internal Comp(TypeExpression expression)
    {
        Expression = expression;
    }

    public bool Matches<T>(Comp<T> other) => Expression.Matches(other.Expression);
    public bool Matches(Comp other) => Expression.Matches(other.Expression);
}

/// <summary>
/// Component Expression for Component types (of any kind).
/// </summary>
/// <remarks>
/// Variables of this type describe a Component, Relation, or Link, but not the actual values.
/// </remarks>
/// <param name="match">optional match expression for relation-backing components</param>
/// <typeparam name="T">any type</typeparam>
public readonly record struct Comp<T>(Match match = default)
{
    internal TypeExpression Expression => TypeExpression.Of<T>(match);

    /// <summary>
    /// The size of this component for SIMD operations, in bytes.
    /// If 0, the component is managed or not blittable, and cannot be used for SIMD.
    /// </summary>
    public int SIMDsize => Expression.SIMDsize;
    
    /// <summary>
    /// Component Expression for a blittable type with a specific relation target (match expression).
    /// </summary>
    public static Comp<T> Matching(Match target) => new(target);

    /// <summary>
    /// Plain component expression for a blittable type.
    /// </summary>
    public static Comp<T> Plain => new(default);

    /// <summary>
    /// Does this Component match another Component Expression?
    /// </summary>
    public bool Matches(Comp<T> other) => Expression.Matches(other.Expression);
    
    /// <summary>
    /// Cast this component to the typeless representation used by filters, etc.
    /// (this representation wraps an opaque internal type of the ECS)
    /// </summary>
    public static implicit operator Comp(Comp<T> self) => new(self.Expression);
}
