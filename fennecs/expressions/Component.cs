using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs;

/// <summary>
/// A boxed Component Expression with its accompanying Type and Value
/// </summary>
/// <remarks>
/// See <see cref="Comp{T}"/> for strongly-typed Component Expressions.
/// </remarks>
public readonly record struct Component
{
    /// <summary>
    /// Is this Component a Relation? (if true, targetEntity returns a valid Entity).
    /// </summary>
    /// <remarks>
    /// If targetEntity is despawned, Component instances involving relations with that Entity already in existence remain unaffected.
    /// </remarks>
    public bool isRelation => Expression.Match.IsEntity;
    
    /// <summary>
    /// Is this Component a Link? (if true, the Value is the linked Object)
    /// </summary>
    public bool isLink => Expression.Match.IsObject;
    
    /// <summary>
    /// The Entity target of this Component, if it is a Relation.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Component is not a Relation</exception>
    public Entity targetEntity
    {
        get
        {
            if (Expression.isRelation) return new(World, Expression.Identity);
            throw new InvalidOperationException("Component is not a relation.");
        }
    }
    
    /// <summary>
    /// The backing Type of this Component.
    /// </summary>
    public Type Type => Expression.Type;

    /// <summary>
    /// The boxed Value of this Component.
    /// </summary>
    /// <remarks>
    /// This is guaranteed to be assignable to the backing System.<see cref="Type"/> used by the component.
    /// </remarks>
    public IStrongBox Box { get; }
    
    private World World { get; }
    private TypeExpression Expression { get; }
    
    internal Component(TypeExpression expression, IStrongBox box, World world)
    {
        World = world;
        Expression = expression;
        Box = box;
    }
    
    #region DEPRECATED
    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with or without a Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Any)")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> Any<T>() => new(Match.Any);

    /// <summary>
    /// Strongly-Typed Wildcard for a specific component type, with any (but not no) Target. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Match.Target)")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> AnyRelation<T>() => new(Match.Target);

    /// <summary>
    /// Wildcard for a specific component type, with any Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Entity.Any)")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> AnyEntity<T>() => new(Match.Entity);

    /// <summary>
    /// Strongly-Typed for a specific component type, with any Object Link. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.Any)")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> AnyObject<T>() => new(Link.Any); //new(Match.Object);

    /// <summary>
    /// Strongly-Typed for a specific component type, with no Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Plain")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> PlainComponent<T>() => Comp<T>.Plain;

    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Entity-Entity Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(target)")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> SpecificEntity<T>(Entity target) => Comp<T>.Matching(target);

    /// <summary>
    /// Strongly-Typed for a specific component type, with a specific Object Link Relation. Used for Stream Filtering and CRUD.
    /// </summary>
    [Obsolete("use Comp<T>.Matching(Link.With(target))")]
    [ExcludeFromCodeCoverage]
    public static Comp<T> SpecificLink<T>(T target) where T : class => Comp<T>.Matching(Link.With(target));
    #endregion
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
    /*
    /// <summary>
    /// The backing Type of this Component Expression.
    /// </summary>
    public Type Type => Expression.Type;

    /// <summary>
    /// Match against a strongly typed Component Expression.
    /// </summary>
    public bool Matches<T>(Comp<T> other) => Expression.Matches(other.Expression);

    /// <summary>
    /// Match against a generic Component Expression.
    /// </summary>
    public bool Matches(Comp other) => Expression.Matches(other.Expression);
    */
    
    internal readonly TypeExpression Expression;

    internal Comp(TypeExpression expression)
    {
        Expression = expression;
    }

}

/// <summary>
/// Component Expression for Component types (of any kind). This is used as a more deliberate way to create and apss Component expressions around, for example for SIMD and Stream filters.
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
    internal int SIMDsize => Expression.SIMDsize;

    /// <summary>
    /// Component Expression for a blittable type with a specific relation target (match expression).
    /// </summary>
    public static Comp<T> Matching(Match target) => new(target);

    /// <summary>
    /// Plain component expression for a blittable type.
    /// </summary>
    public static Comp<T> Plain => new(default);

    /// <summary>
    /// A component expression matching a specific link object.
    /// </summary>
    public static Comp<U> Matching<U>(U target) where U : class => new(Match.Link(target));

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
