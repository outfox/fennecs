using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Secondary Key for a Component type expression - used in relations, object links, etc.
/// </summary>
public readonly record struct Key
{
    internal ulong Value { get; }
    
    internal Key(ulong value)
    {
        Debug.Assert((value & HeaderMask) == 0, "Keys may not have header bits set.");
        Value = value;
    }

    public Kind Category => (Kind) ((Value & CategoryMask) >> 44);
    
    
    /// <summary>
    /// Create a Key for an Entity-Entity relationship.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of(Entity entity) => new(entity.Id.Value & KeyMask); /* KeyCategory already set by Entity*/

    /// <summary>
    /// Create a Key for a tracked object and the backing Object Link type.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of<L>(L link) where L : class => new((ulong) Kind.Link | LanguageType<L>.LinkId | (uint) link.GetHashCode());
    
    internal const ulong HeaderMask = 0xFFFF_0000_0000_0000u;
    internal const ulong KeyMask = ~0xFFFF_0000_0000_0000u;
    internal const ulong CategoryMask = 0x0000_F000_0000_0000u;
    
    /// <summary>
    /// Category of the Key.
    /// </summary>
    public enum Kind : ulong
    {
        /// <summary>
        /// Plain Component (no secondary key / relations)
        /// </summary>
        Plain = default,
        
        /// <summary>
        /// Specific Entity
        /// </summary>
        Entity = 0x0000_E000_0000_0000u,

        /// <summary>
        /// Specific Object Link
        /// </summary>
        Link = 0x0000_1000_0000_0000u,
        
        
        //TODO: Move these into Match!!!
        
        /// <summary>
        /// wildcard (any Object Link)
        /// </summary>
        AnyLink = 0x0000_A000_0000_0000u,

        /// <summary>
        /// wildcard (any Entity relation)
        /// </summary>
        AnyEntity = 0x0000_B000_0000_0000u,

        /// <summary>
        /// wildcard (anything except Plain)
        /// </summary>
        AnyTarget = 0x0000_C000_0000_0000u,
        
        /// <summary>
        /// wildcard (anything, including Plain)
        /// </summary>
        Any = 0x0000_F000_0000_0000u,
    }
    
    
    /// <summary>
    /// Is this Key representing an Entity Relation?
    /// </summary>
    public bool IsEntity => Category == Kind.Entity;
    
    /// <summary>
    /// Is this Key representing an Object Link?
    /// </summary>
    public bool IsLink => Category == Kind.Link;


    #region Wildcards

    /// <summary>
    /// <para>
    /// <c>default</c><br/>In Query Matching; matches ONLY Plain Components, i.e. those without any Secondary Key Target.
    /// </para>
    /// <para>
    /// Since it's specific, this Match Expression is always free and has no enumeration cost.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Not a wildcard. Formerly known as "None", as plain components without a target
    /// can only exist once per Entity (same as components with a particular target).
    /// </remarks>
    public static readonly Key None = default;

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
    public static readonly Key Any = new((ulong) Kind.Any);

    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Match"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static readonly Key Target = new((ulong) Kind.AnyTarget);


    //public static readonly Key Entity = new((ulong) KeyCategory.Entity);


    #endregion


    /// <inheritdoc />
    public override string ToString()
    {
        if (Equals(None))
            return "[None]";

        if (Equals(Any))
            return "wildcard[Any]";

        if (Equals(Target))
            return "wildcard[Target]";

        if (Equals(Identity.Any))
            return "wildcard[Entity]";

        if (Equals(Link.AnyLink))
            return "wildcard[Link]";

        switch (Category)
        {
            case Kind.Entity:
                    return new LiveEntity(this).ToString();
        }

        return $"?-{Value:x16}";
    }
}

/// <summary>
/// A LiveEntity is a special Identity that's guaranteed to be alive.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly ref struct LiveEntity
{
    [FieldOffset(0)]
    private readonly ulong Raw;

    [FieldOffset(0)]
    internal readonly int Index;

    [FieldOffset(4)]
    private readonly World.Id WorldId;

    /// <summary>
    /// An Entity that is currently alive, for the purpose of being used as a relation key.
    /// </summary>
    public LiveEntity(Key key)
    {
        Debug.Assert(key.Category == Key.Kind.Entity, $"Cannot create LiveEntity from non-Entity {key}!");
        Debug.Assert(key.IsEntity, $"Cannot create LiveEntity from non-Entity {key}!");
        Raw = key.Value;
    }

    /// <summary>
    /// An Entity that is currently alive, for the purpose of being used as a relation key.
    /// </summary>
    public LiveEntity(Identity id)
    {
        Debug.Assert(id.Alive, $"Cannot create LiveEntity from dead-Entity {id}!");
        Raw = id.Value & Key.KeyMask;
    }

    /// <summary>
    /// Returns the actual Entity this LiveEntity refers to.
    /// </summary>
    /// <param name="self">a LiveEntity</param>
    /// <returns>the entity</returns>
    public static implicit operator Identity(LiveEntity self) => self.World[self];
    
    public World World => World.Get(WorldId);

    /// <inheritdoc />
    public override string ToString() => $"E-{WorldId}:{Index:x8} live";
}