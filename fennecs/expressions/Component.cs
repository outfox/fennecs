namespace fennecs;

/// <summary>
/// A specific set of match expressions to match specific component types where type parameters are not available.
/// </summary>
public record Component
{
    internal Component(TypeExpression type)
    {
        Type = type;
    }
    
    internal TypeExpression Type { get; }

    internal bool Matches(Component other) => Type.Matches(other.Type);

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
    
    /// <summary>
    /// Does this Component match another Component Expression?
    /// </summary>
    public bool Matches(Component other) => TypeExpression.Matches(other.Type);

    /// <summary>
    /// Does this Component match another Component Expression?
    /// </summary>
    public bool Matches(Comp<T> other) => TypeExpression.Matches(other.TypeExpression);

    /// <summary>
    /// Convert to a generic Component Expression.
    /// </summary>
    public static implicit operator Component(Comp<T> self ) => new(self.TypeExpression);
}
