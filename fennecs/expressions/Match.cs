using System.Diagnostics;

namespace fennecs;

/// <summary>
/// Used to match against secondary Keys in Type Expressions (e.g. Queries, Streams, Filters, Masks).
/// </summary>
/// <para>
/// Match's static readonly constants differentiate between Plain Components, Entity-Entity Relations, and Entity-Object Relations.
/// The class offers a set of Wildcards for matching combinations of the above in <see cref="Query">Queries</see>; as opposed to filtering for only a specific target.
/// </para>
public readonly record struct Match
{
    private readonly ulong _value;

    private enum Wildcard : ulong
    {
        /// <summary>
        /// wildcard (any Object Link)
        /// </summary>
        Link = 0x0000_A000_0000_0000u,

        /// <summary>
        /// wildcard (any Entity relation)
        /// </summary>
        Entity = 0x0000_B000_0000_0000u,

        /// <summary>
        /// wildcard (anything except Plain)
        /// </summary>
        Target = 0x0000_C000_0000_0000u,
        
        /// <summary>
        /// wildcard (anything, including Plain)
        /// </summary>
        Any = 0x0000_F000_0000_0000u,
    }
    
    internal Match(Key key) => _value = key.Value;

    /// <summary>
    /// <para>
    /// Match Expression to match only a specific Relation (Entity-Entity).
    /// </para>
    /// <para>Use it freely in filter expressions. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// </summary>
    public static Match Relation(Identity other) => new(other.Key);
    

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches all types of relations on the given Stream Type: <b>Plain, Entity, and Object</b>.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Using wildcards can lead to a CROSS JOIN effect, iterating over entities multiple times for
    /// each matching component. While querying is efficient, this increases the number of operations per entity.</para>
    /// <para>This is an intentional feature, and the user is protected by the fact that the default is <see cref="Plain"/>.</para>
    /// <para>This effect is more pronounced in large archetypes with many matching components, potentially
    /// multiplying the workload significantly. However, for smaller archetypes or simpler tasks, impacts are minimal.</para>
    /// <para>Risks and considerations include:</para>
    /// <ul>
    /// <li>Repeated enumeration: Entities matching a wildcard are processed multiple times, for each matching
    /// component type combination.</li>
    /// <li>Complex queries: Especially in Archetypes where Entities match multiple components, multiple wildcards
    /// can create a cartesian product effect, significantly increasing complexity and workload.</li>
    /// <li>Use wildcards deliberately and sparingly.</li>
    /// </ul>
    /// </remarks>
    public static readonly Match Any = new(new((ulong) Wildcard.Any));
    //public static Match Any => new(Identity.Any); // or prefer default ?

    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Match"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Target = new(new((ulong) Wildcard.Target));
    
    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Link = new(new((ulong) Wildcard.Link));

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches all <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Match Entity => new(new((ulong) Wildcard.Entity));


    /// <summary>
    /// <para><b>Plain Component match expression for Entity iteration.</b><br/>This matches only <b>Plain</b> Components of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>This expression does not result in multiple enumeration, because it's not technically a Wildcard - there can only be one plain component per type on an Entity.</para>
    /// </summary>
    /// <inheritdoc cref="Plain"/>
    public static Match Plain => new(new(default));
    

    /// <summary>
    /// <para>Implicitly convert an <see cref="Identity"/> to a <see cref="Match"/> for use in filter expressions.</para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    //public static implicit operator Match(Identity value) => new(value);
    public static implicit operator Match(Identity value) => new(value.Key);


    /// <inheritdoc/>
    public override string ToString()
    {
        return _value switch
        {
            (ulong) Wildcard.Any => "wildcard[Any]",
            (ulong) Wildcard.Target => "wildcard[Target]",
            (ulong) Wildcard.Entity => "wildcard[Entity]",
            (ulong) Wildcard.Link => "wildcard[Link]",
            _ => new Key(_value).ToString(),
        };
    }

    /// <summary>
    /// Is this Match Expression a Wildcard?
    /// </summary>
    public bool IsWildcard => _value switch
    {
        (ulong) Wildcard.Any => true,
        (ulong) Wildcard.Target => true,
        (ulong) Wildcard.Entity => true,
        (ulong) Wildcard.Link => true,
        _ => false,
    };

    public Key Key => IsWildcard ? throw new InvalidOperationException("Cannot get Key of a Wildcard Match Expression.") : new Key(_value);

    public bool IsLink => _value == (ulong) Wildcard.Link || new Key(_value).IsLink;
    public bool IsEntity => _value == (ulong) Wildcard.Entity || new Key(_value).IsEntity;
}
