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
    internal readonly ulong Value;

    internal enum Wildcard : ulong
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
    
    internal Match(Key key) => Value = key.Value;
    
    internal Match(Wildcard wildcard) => Value = (ulong) wildcard;

    /// <summary>
    /// <para>
    /// Match Expression to match only a specific Relation (Entity-Entity).
    /// </para>
    /// <para>Use it freely in filter expressions. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// </summary>
    public static Match Relation(Entity other) => new(other.Key);
    

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
    public static Match Any { get; }  = new(Wildcard.Any);
    //public static Match Any => new(Entity.Any); // or prefer default ?

    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Key"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Target = new(Wildcard.Target);
    
    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Link = new(Wildcard.Link);

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches all <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Match Entity = new(Wildcard.Entity);


    /// <summary>
    /// <b>Match Expression to match only Plain Components.</b>
    /// <i>(components without a secondary key)</i>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>This expression does not result in multiple enumeration, because it's not technically a Wildcard - there can only be one plain component per type on an Entity.</para>
    /// </summary>
    /// <inheritdoc cref="Plain"/>
    public static readonly Match Plain = default;
    

    /// <summary>
    /// <para>Implicitly convert an <see cref="Entity"/> to a <see cref="Match"/> for use in filter expressions.</para>
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    //public static implicit operator Match(Entity value) => new(value);
    public static implicit operator Match(Entity entity) => new(entity.Key);


    /// <inheritdoc/>
    public override string ToString()
    {
        return Value switch
        {
            (ulong) Wildcard.Any => "wildcard[Any]",
            (ulong) Wildcard.Target => "wildcard[Target]",
            (ulong) Wildcard.Entity => "wildcard[Entity]",
            (ulong) Wildcard.Link => "wildcard[Link]",
            _ => new Key(Value).ToString(),
        };
    }

    /// <summary>
    /// Is this Match Expression a Wildcard?
    /// </summary>
    public bool IsWildcard => Value switch
    {
        (ulong) Wildcard.Any => true,
        (ulong) Wildcard.Target => true,
        (ulong) Wildcard.Entity => true,
        (ulong) Wildcard.Link => true,
        _ => false,
    };

    /// <summary>
    /// Does this Match expression match any Object Links?
    /// </summary>
    public bool IsLink =>
        Value switch
        {
            (ulong) Wildcard.Link => true,
            (ulong) Wildcard.Target => true,
            (ulong) Wildcard.Any => true,
            _ => new Key(Value).IsLink,
        };

    /* TODO: Likely not needed
    /// <summary>
    /// The Key of this Match Expression (for use in relations).
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public Key Key => IsWildcard ? throw new InvalidOperationException("Cannot get Key of a Wildcard Match Expression.") : new Key(Value);

    public bool IsLink => Value == (ulong) Wildcard.Link || new Key(Value).IsLink;
    public bool IsEntity => Value == (ulong) Wildcard.Entity || new Key(Value).IsEntity;
    */
}

/// <summary>
/// Helpers for creating secondary keys for Match Expressions.
/// This is a convenience class to provide a different syntax.
/// The contained Match terms are equivalent to the ones in <see cref="Match"/> and <see cref="Entity"/>.
/// </summary>
public static class Any
{
    /// <summary>
    /// Match Expression to match any Entity secondary keys relations.
    /// </summary>
    /// <remarks>
    /// Same as <see cref="Entity.Any"/> or <see cref="Match.Entity"/>
    /// </remarks>
    public static readonly Match Entity = fennecs.Entity.Any;

    /// <summary>
    /// Match Expression to match any Object Links.
    /// </summary>
    /// <remarks>
    /// Same as <see cref="Link.Any"/> or <see cref="Match.Link"/>
    /// </remarks>
    public static readonly Match Link = fennecs.Link.Any;

    /// <summary>
    /// Match Expression to match any non-default secondary keys.
    /// </summary>
    /// <remarks>
    /// Same as <see cref="Match.Target"/>
    /// </remarks>
    public static readonly Match Target = Match.Target;
    
    /// <summary>
    /// Match Expression to match any Component types, including Plain Components.
    /// </summary>
    /// <remarks>
    /// Same as <see cref="Match.Any"/>
    /// </remarks>
    public static readonly Match All = Match.Any;
}