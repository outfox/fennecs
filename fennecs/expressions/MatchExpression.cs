using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Strongly-typed way to match against a specific Component, Relation, or Object Link.
/// It is used in <see cref="Query"/>, in <see cref="Stream"/>s and their Filters, etc.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
[DebuggerDisplay("{ToString()}")]
public readonly record struct MatchExpression
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [FieldOffset(0)] internal readonly ulong _value;
    
    [field: FieldOffset(6)] 
    private short TypeId { get; }

    private Match Match => new(_value);
    
    internal static MatchExpression Of<T>(Match match) => new(match, LanguageType<T>.Id);

    internal static MatchExpression Of(Type type, Match match) => new(match, LanguageType.Identify(type));

    internal static MatchExpression Of<L>(L link) where L : class => new(Key.Of(link), LanguageType<L>.Id);
    
    private MatchExpression(Match match, short typeId)
    {
        _value = match.Value;
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
    public bool IsRelation => Match != default(Key);


    /// <summary>
    ///  Is this TypeExpression a Wildcard expression? See <see cref="Cross"/>.
    /// </summary>
    public bool IsWildcard => Match.IsWildcard;

    
    /// <summary>
    /// Match against another TypeExpression; used for Query Matching.
    /// Examines the Type and Target fields of either and decides whether the other TypeExpression is a match.
    /// <para>
    /// See also: <see cref="Key.Plain"/>, <see cref="Entity.Any"/>, <see cref="Match.Target"/>, <see cref="Match.Link"/>, <see cref="Match.Any"/>
    /// </para>
    /// </summary>
    /// <param name="other">another type expression</param>
    /// <returns>true if the other expression is matched by this expression</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        return (Match.Value) switch
        {
            // Match.None matches only None. (plain Components)
            default(ulong) => other.Key == default,
            
            // Match.Any matches everything; relations and pure Components (target == none).
            (ulong) Match.Wildcard.Any => true,

            // Match.Target matches all Entity-Target Relations.
            (ulong) Match.Wildcard.Target => other.Key != default,
            
            // Match.Relation matches only Entity-Entity relations.
            (ulong) Match.Wildcard.Entity => other.Key.IsEntity,

            // Match.Link matches only Entity-Object relations.
            (ulong) Match.Wildcard.Link => other.Key.IsLink,

            // Direct match?
            _ => Match == other.Key,
        };
    }

    /// <inheritdoc cref="Matches(TypeExpression)"/>
    /// <returns>false if the other expression is matched by this expression</returns>
    public bool MatchesNot(TypeExpression other) => !Matches(other);

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

        //TODO: HUGE OPTIMIZATION POTENTIAL! (bloom filter + set comparison is way faster than naive linear search, etc.) FIXME!!
        return other.Any(type => self.Matches(type));
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return Match != default ? $"<{LanguageType.Resolve(TypeId)}> >> {Match}" : $"<{LanguageType.Resolve(TypeId)}> (plain)";
    }

    internal TypeExpression AsTypeExpression()
    {
        //TODO: This should probably be an Overload in Signature (used in Signature.Remove)
        Debug.Assert(!IsWildcard, "Can't cast a Wildcard to a TypeExpression");
        return new(_value);
    }
}