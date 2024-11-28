using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Strongly-typed way to match against a specific Component, Relation, or Object Link.
/// It is used in <see cref="Query"/>, in <see cref="Stream"/>s and their Filters, etc.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
[SkipLocalsInit]
public readonly record struct MatchExpression
{
    [FieldOffset(0)]
    private readonly ulong _value;

    [field: FieldOffset(0)] 
    private Match Match { get; init; }
    
    [field: FieldOffset(6)] 
    internal short TypeId { get; }
    
    internal static MatchExpression Of<T>(Match match) => new(match, LanguageType<T>.Id);
    internal static MatchExpression Of(Type type, Match match) => new(match, LanguageType.Identify(type));

    private MatchExpression(Match match, short typeId)
    {
        _value = match.Value;
        TypeId = typeId;
    }
    
    private MatchExpression(Key key, short typeId)
    {
        _value = key.Value;
        TypeId = typeId;
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
    /// See also: <see cref="fennecs.Identity.Plain"/>, <see cref="fennecs.Identity.Target"/>, <see cref="fennecs.Identity.Entity"/>, <see cref="fennecs.Identity.Object"/>, <see cref="fennecs.Identity.Any"/>
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
    /// <seealso cref="fennecs.Identity.Plain"/>
    /// <seealso cref="fennecs.Identity.Target"/>
    /// <seealso cref="fennecs.Identity.Entity"/>
    /// <seealso cref="fennecs.Identity.Object"/>
    /// <seealso cref="fennecs.Identity.Any"/>
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
    /// Expands this TypeExpression into a set of TypeExpressions that that are Equivalent but unique.
    /// </summary>
    /// <remarks>
    /// <ul>
    /// <li>wild Any -> [ wild Plain, wild Entity, wild Object ]</li>
    /// <li>wild Target -> [ wild Entity, wild Object ]</li>
    /// <li>specific Object -> [ wild Object ] </li>
    /// <li>specific Entity -> [ wild Entity ]</li>
    /// </ul>
    /// </remarks>
    public HashSet<MatchExpression> Expand()
    {
        //TODO: This can be optimized by either compile time constants, or using Bloom filters.
        if (Match == Match.Any)
            return
            [
                this with {Match = default}, 
                this with {Match = Match.Entity}, 
                this with {Match = Match.Link},
                this with {Match = Match.Target}
            ];

        if (Match == Match.Target)
            return [
                this with {Match = Match.Any}, 
                this with {Match = Match.Entity}, 
                this with {Match = Match.Link}
            ];

        if (Match == Match.Entity)
            return [
                this with { Match = Match.Any }, 
                this with { Match = Match.Target }
            ];

        if (Match == Match.Link) 
            return [
                this with { Match = Match.Any }, 
                this with { Match = Match.Target }
            ];

        if (Match.IsLink) 
            return [
                this with { Match = Match.Any }, 
                this with { Match = Match.Target }, 
                this with { Match = Match.Link }
            ];

        if (Match.IsEntity) 
            return [
                this with { Match = Match.Any }, 
                this with { Match = Match.Target }, 
                this with { Match = Match.Entity }
            ];

        return [this with { Match = Match.Any }];
    }
   
}