namespace fennecs;

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

    internal bool Matches(Component other) => value.Matches(other.value);

    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with or without a Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Any)")]
    public static Component AnyAny<T>() => new(TypeExpression.Of<T>(new(new(-1, 0))));
    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with any (but not no) Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Target)")]
    public static Component AnyRelation<T>() => new(TypeExpression.Of<T>(new(new(-2, 0))));
    /// <summary>
    /// Wildcard for a specific component type, with any Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Entity.Any)")]
    public static Component AnyEntity<T>() => new(TypeExpression.Of<T>(new(new(-3, 0))));
    /// <summary>
    /// Strongly-Typed for a specific component type, with any Object Link. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.Any)")]
    public static Component AnyObject<T>() => new(TypeExpression.Of<T>(new(new(-4, 0))));
    /// <summary>
    /// Strongly-Typed for a specific component type, with no Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Plain")]
    public static Component PlainComponent<T>() => new(TypeExpression.Of<T>(new(default)));
    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(target)")]
    public static Component SpecificEntity<T>(Entity target) => new(TypeExpression.Of<T>(target));
    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Object Link Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.With(target))")]
    public static Component SpecificLink<T>(T target) where T : class => new(TypeExpression.Of<T>(fennecs.Link.With(target)));
}


/// <summary>
/// Interface tagging blittable Component Expressions.
/// </summary>
public interface IBlittable
{
    internal TypeExpression TypeExpression { get; }
}


/// <summary>
/// Component Expression for Component types (of any kind).
/// </summary>
/// <param name="match">optional match expression for relation-backing components</param>
/// <typeparam name="T">any type</typeparam>
public readonly record struct Comp<T>(Match match = default)
{
    internal TypeExpression TypeExpression => TypeExpression.Of<T>(match);

    /// <summary>
    /// The size of this component for SIMD operations, in bytes.
    /// If 0, the component is managed or not blittable, and cannot be used for SIMD.
    /// </summary>
    public int SIMDsize => TypeExpression.SIMDsize;

    /// <summary>
    /// Component Expression for a blittable type with a specific relation target (match expression).
    /// </summary>
    public static Comp<T> Matching(Match target) => new(target);

    /// <summary>
    /// Plain component expression for a blittable type.
    /// </summary>
    public static Comp<T> Plain => new(default);
}
