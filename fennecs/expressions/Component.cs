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
    public bool isRelation => Expression.Key.IsEntity;
    
    /// <summary>
    /// Is this Component a Link? (if true, the Value is the linked Object)
    /// </summary>
    public bool isLink => Expression.Key.IsLink;
    
    /// <summary>
    /// The Entity target of this Component, if it is a Relation.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Component is not a Relation</exception>
    public Identity targetEntity
    {
        get
        {
            if (Expression.Key.IsEntity) return new(World, Expression.TargetEntity);
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
    internal TypeExpression Expression { get; }
    
    internal Component(TypeExpression expression, IStrongBox box, World world)
    {
        World = world;
        Expression = expression;
        Box = box;
    }
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

    /// <summary>
    /// Creates a Comp expression for a specific type, potentially determined at runtime.
    /// </summary>
    public static Comp Virtual(Type type, Match match = default) => new(TypeExpression.Of(type, match));
}

/// <summary>
/// Component Expression for Component types (of any kind). This is used as a more deliberate way to create and apss Component expressions around, for example for SIMD and Stream filters.
/// </summary>
/// <remarks>
/// Variables of this type describe a Component, Relation, or Link, but not the actual values.
/// </remarks>
/// <param name="Key">optional secondary <see cref="Key"/></param>
/// <typeparam name="T">any type</typeparam>
public readonly record struct Comp<T>(Key Key = default)
{
    internal TypeExpression Expression => TypeExpression.Of<T>(Key);

    /// <summary>
    /// Component Expression for a blittable type with a specific relation target (match expression).
    /// </summary>
    public static Comp<T> Matching(Key target) => new(target);

    /// <summary>
    /// Plain component expression for a blittable type.
    /// </summary>
    public static Comp<T> Plain => new(default);

    /// <summary>
    /// A component expression matching a specific link object.
    /// </summary>
    public static Comp<U> Matching<U>(U target) where U : class => new(Key.Of(target));

    /// <summary>
    /// Does this Component match another Component Expression?
    /// </summary>
    public bool Matches(Comp<T> other) => Expression == other.Expression;

    /// <summary>
    /// Cast this component to the typeless representation used by filters, etc.
    /// (this representation wraps an opaque internal type of the ECS)
    /// </summary>
    public static implicit operator Comp(Comp<T> self) => new(self.Expression);
}
