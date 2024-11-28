namespace fennecs;

/// <summary>
/// Match Expressions
/// </summary>
/// <para>
/// Match's static readonly constants differentiate between Plain Components, Entity-Entity Relations, and Entity-Object Relations.
/// The class offers a set of Wildcards for matching combinations of the above in <see cref="Query">Queries</see>; as opposed to filtering for only a specific target.
/// </para>
public readonly record struct Match
{
    private readonly ulong _value;
    
    internal Match(Key key) => _value = key.Value;

    /// <summary>
    /// <para>
    /// Match Expression to match only a specific Relation (Entity-Entity).
    /// </para>
    /// <para>Use it freely in filter expressions. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// </summary>
    public static Match Relation(Identity other) => new(other.Key);
    
    /// <summary>
    /// <para>
    /// Match Expression to match only a specific Object Link (Entity-Object).
    /// </para>
    /// <para>Use it freely in filter expressions. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// </summary>
    public static Match Link<T>(T link) where T : class => new(Identity.Of(link));

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
    public static Match Any => new(Identity.Any); // or prefer default ?

    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Match"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Match Target => new(Identity.Target);
    
    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Match Object => new(Identity.Object);

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches all <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Match Entity => new(Identity.Entity);


    /// <summary>
    /// <para><b>Plain Component match expression for Entity iteration.</b><br/>This matches only <b>Plain</b> Components of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>This expression does not result in multiple enumeration, because it's not technically a Wildcard - there can only be one plain component per type on an Entity.</para>
    /// </summary>
    /// <inheritdoc cref="Plain"/>
    public static Match Plain => new(Identity.Plain);
    

    /// <summary>
    /// <para>Implicitly convert an <see cref="Identity"/> to a <see cref="Match"/> for use in filter expressions.</para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    //public static implicit operator Match(Identity value) => new(value);
    public static implicit operator Match(Entity value) => new(value);

    //public static implicit operator Match(Identity value) => new(value);
    
    //TODO Maybe not even needed...
    internal bool IsWildcard => Key.IsWildcard;
    internal bool IsEntity => Key.IsEntity;
    internal bool IsObject => Key.IsObject;


    /// <inheritdoc/>
    public override string ToString() => Key.ToString();
}
