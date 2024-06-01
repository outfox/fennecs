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
    internal Identity Value { get; init; }
    
    internal Match(Identity Value) => this.Value = Value;

    public static Match Relation(Entity other) => new(other.Id);
    public static Match Link<T>(T link) where T : class => new(Identity.Of<T>(link));

    public static Match Any => new(idAny); // or prefer default ?
    public static Match Target => new(idTarget);
    public static Match Object => new(idObject);
    public static Match Entity => new(idEntity);
    public static Match Plain => new(idPlain);
    
    public bool Matches(Match other) => Value == other.Value;
    
    public static implicit operator Match(Identity value) => new(value);
    public static implicit operator Match(Entity value) => new(value);
    
    //TODO Maybe not even needed...
    public bool IsWildcard => Value.IsWildcard;
    public bool IsEntity => Value.IsEntity;
    public bool IsObject => Value.IsObject;
    public bool IsTarget => Value == idTarget;
    public bool IsPlain => Value == idPlain;
    
    [Obsolete("Maybe find a way to remove me.")]
    internal ulong Raw => Value.Value;
    
    internal void Deconstruct(out Identity identity)
    {
        identity = Value;
    }
    
    
    /// <summary>
    /// <para>
    /// <c>default</c><br/>In Query Matching; matches ONLY Plain Components, i.e. those without a Relation Target.
    /// </para>
    /// <para>
    /// Since it's specific, this Match Expression is always free and has no enumeration cost.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Not a wildcard. Formerly known as "None", as plain components without a target
    /// can only exist once per Entity (same as components with a particular target).
    /// </remarks>
    internal static readonly Identity idPlain = new(int.MinValue, 0); // was default
    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    internal static readonly Identity idEntity = new(-3, 0);
    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    internal static readonly Identity idObject = new(-4, 0);
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
    /// <para>This is an intentional feature, and <c>Match.Any</c> is the default as usually the same backing types are not re-used across
    /// relations or links; but if they are, the user likely wants their Query to enumerate all of them.</para>
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
    internal static readonly Identity idAny = default; //was: new(-1, 0);
    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Target"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    internal static readonly Identity idTarget = new(-2, 0);
}
