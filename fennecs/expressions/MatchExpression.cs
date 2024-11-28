using System.Runtime.InteropServices;

namespace fennecs;

[StructLayout(LayoutKind.Explicit)]
public class MatchExpression
{
    [FieldOffset(0)]
    private readonly ulong _value;
    
    [field: FieldOffset(0)] 
    internal Match Match { get; init; }
    
    [field: FieldOffset(6)] 
    internal short TypeId { get; }
    
    public Type Type => LanguageType.Resolve(TypeId);
    
    public bool IsWildcard => Match.IsWildcard;

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
        if (Match == Match.Plain) return other.Match == Match.Plain;

        // Match.Any matches everything; relations and pure Components (target == none).
        if (Match == Match.Any) return true;

        // Match.Target matches all Entity-Target Relations.
        if (Match == Match.Target) return other.Match != Match.Plain;

        // Match.Relation matches only Entity-Entity relations.
        if (Match == Match.Entity) return other.Match.IsEntity;

        // Match.Object matches only Entity-Object relations.
        if (Match == Match.Link) return other.Match.IsLink;

        // Direct match?
        return Match == other.Match;
    }
}