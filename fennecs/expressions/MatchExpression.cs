using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Strongly-typed way to match against a specific Component, Relation, or Object Link.
/// It is used in <see cref="Query"/>, in <see cref="Stream"/>s and their Filters, etc.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct MatchExpression
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    [FieldOffset(0)] internal readonly ulong _value;
    
    [field: FieldOffset(0)] 
    private Match Match { get; init; }
    
    [field: FieldOffset(6)] 
    private short TypeId { get; }
    
    internal static MatchExpression Of<T>(Match match) => new(match, LanguageType<T>.Id);
    internal static MatchExpression Of<T>(Key key) => new(key, LanguageType<T>.Id);

    internal static MatchExpression Of(Type type, Match match) => new(match, LanguageType.Identify(type));
    internal static MatchExpression Of(Type type, Key key) => new(key, LanguageType.Identify(type));
    

    private MatchExpression(Match match, short typeId)
    {
        Match = match;
        TypeId = typeId;
    }

    /// <summary>
    /// Create a MatchExpression for a specific Component Type.
    /// </summary>
    public MatchExpression(TypeExpression type)
    {
        _value = type._value;
    }

    /// <summary>
    /// The backing Type of the Components this Expression tries to match.
    /// </summary>
    public Type Type => LanguageType.Resolve(TypeId);
    
    /// <summary>
    /// The <see cref="TypeExpression"/> is a relation, meaning it has a target other than None.
    /// </summary>
    public bool isRelation => Match != Match.Plain;


    /// <summary>
    ///  Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool isWildcard => Match.IsWildcard;

    
    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Target fields of either and decides whether the other TypeExpression is a match.
    /// <para>
    /// See also: <see cref="fennecs.Entity.Plain"/>, <see cref="fennecs.Entity.Target"/>, <see cref="Entity"/>, <see cref="fennecs.Entity.Object"/>, <see cref="fennecs.Entity.Any"/>
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// ⚠️ This comparison is non-commutative; the order of the operands matters!
    /// </para>
    /// <para>
    /// You must handle matching the commuted case(s) in your code if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Non-Commutative: <br/><c>Match.Plain</c> doesn't match wildcard <c>Match.Any</c>, but <c>Match.Any</c> <i><b>does</b> match</i> <c>Match.Plain</c>.
    /// </para>
    /// <para>
    /// Pseudo-Commutative: <br/><see cref="Key"/> <c>E-0000007b:00456</c> matches itself, as well as the three wildcards <c>Match.Target</c>, <c>Match.Entity</c>, and <c>Match.Any</c>. Vice versa, it is also matched by all of them! 
    /// </para>
    /// </example>
    /// <param name="other">another type expression</param>
    /// <seealso cref="fennecs.Entity.Plain"/>
    /// <seealso cref="fennecs.Entity.Target"/>
    /// <seealso cref="Entity"/>
    /// <seealso cref="fennecs.Entity.Object"/>
    /// <seealso cref="fennecs.Entity.Any"/>
    /// <seealso cref="Match.Relation"/>
    /// <seealso cref="Match.Link{T}"/>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Match.None matches only None. (plain Components)
        if (Match == Match.Plain) return other.Key == default;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Match == Match.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Match == Match.Target) return other.Key != default;

        // Match.Relation matches only Entity-Entity relations.
        if (Match == Match.Entity) return other.Key.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Match == Match.Link) return other.Key.IsLink;

        // Direct match?
        return Match == new Match(other.Key);
    }
   
    
    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Target fields of either and decides whether the other TypeExpression is a match.
    /// <para>
    /// See also: <see cref="fennecs.Entity.Plain"/>, <see cref="fennecs.Entity.Target"/>, <see cref="Entity"/>, <see cref="fennecs.Entity.Object"/>, <see cref="fennecs.Entity.Any"/>
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// ⚠️ This comparison is non-commutative; the order of the operands matters!
    /// </para>
    /// <para>
    /// You must handle matching the commuted case(s) in your code if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Non-Commutative: <br/><c>Match.Plain</c> doesn't match wildcard <c>Match.Any</c>, but <c>Match.Any</c> <i><b>does</b> match</i> <c>Match.Plain</c>.
    /// </para>
    /// <para>
    /// Pseudo-Commutative: <br/><see cref="Key"/> <c>E-0000007b:00456</c> matches itself, as well as the three wildcards <c>Match.Target</c>, <c>Match.Entity</c>, and <c>Match.Any</c>. Vice versa, it is also matched by all of them! 
    /// </para>
    /// </example>
    /// <param name="other">another type expression</param>
    /// <seealso cref="fennecs.Entity.Plain"/>
    /// <seealso cref="fennecs.Entity.Target"/>
    /// <seealso cref="Entity"/>
    /// <seealso cref="fennecs.Entity.Object"/>
    /// <seealso cref="fennecs.Entity.Any"/>
    /// <seealso cref="Match.Relation"/>
    /// <seealso cref="Match.Link{T}"/>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(MatchExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        //TODO: This is probably wrong?
        // Match.None matches only None. (plain Components)
        if (Match == Match.Plain) return other.Match == default;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Match == Match.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Match == Match.Target) return other.Match != default;

        // Match.Relation matches only Entity-Entity relations.
        if (Match == Match.Entity) return other.Match.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Match == Match.Link) return other.Match.IsLink;

        // Direct match?
        return Match == other.Match;
    }
   
   
    /// <summary>
    /// TODO: Remove me.
    /// A method to check if a TypeExpression matches any of the given type expressions in an IEnumerable.
    /// Does this <see cref="TypeExpression"/> match any of the given type expressions?
    /// </summary>
    /// <param name="other">a collection of type expressions</param>
    /// <returns>true if matched</returns>
    public bool Matches(IEnumerable<TypeExpression> other)
    {
        var self = this;

        //TODO: HUGE OPTIMIZATION POTENTIAL! (set comparison is way faster than linear search, etc.) FIXME!!
        foreach (var type in other)
        {
            if (self.Matches(type)) return true;
        }

        return false;
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return Match != default ? $"<{LanguageType.Resolve(TypeId)}> >> {Match}" : $"<{LanguageType.Resolve(TypeId)}> (plain)";
    }

    
    /// <summary>
    /// Check if this <see cref="MatchExpression"/> matches any of the given <paramref name="expressions"/>.
    /// </summary>
    public bool Matches(HashSet<MatchExpression> expressions)
    {
        if (Match == Match.Any) return true;
        if (expressions.Contains(this)) return true;

        var self = this;
        return expressions.Any(expr => expr.Matches(self));
    }
}