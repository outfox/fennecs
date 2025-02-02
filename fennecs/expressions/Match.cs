using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Used to match against secondary Keys in Type Expressions (e.g. Queries, Streams, Filters, Masks).
/// </summary>
/// <para>
/// Match's static readonly constants differentiate between Plain Components, Entity-Entity Relations, and Entity-Object Relations.
/// The class offers a set of Wildcards for matching combinations of the above in <see cref="Query">Queries</see>; as opposed to filtering for only a specific target.
/// </para>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct Match
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [FieldOffset(0)] internal readonly ulong Value;
    
    [FieldOffset(0)] internal readonly int Id;
    
    [FieldOffset(4)]
    internal readonly ushort Flags;
    
    public KeyCategory Cat => (KeyCategory)(Flags & Key.CategoryMask);
    
    [Flags]
    public enum KeyCategory : ulong
    {
        /// <summary>
        /// Plain Components (no key)
        /// </summary>
        Plain = default,
        
        /// <summary>
        /// Relation (Target is an entity)
        /// </summary>
        Entity = 1 * 0x0000_0100_0000_0000u,

        /// <summary>
        /// Object Link (target is an instance of its backing type)
        /// </summary>
        Link = 2 * 0x0000_0100_0000_0000u,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        [Obsolete("Reserved for future use.", true)]
        Reserved1 = 4 * 0x0000_0100_0000_0000u,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        [Obsolete("Reserved for future use.", true)]
        Reserved2 = 8 * 0x0000_0100_0000_0000u,
    }

    public enum StorageCategory : ulong
    {
        /// <summary>
        /// Void storage. (used for tags)
        /// </summary>
        Void = default,
        
        /// <summary>
        /// Flat Data storage (most components use this)
        /// </summary>
        Flat = 1 * 0x0000_1000_0000_0000u,

        /// <summary>
        /// Singleton Data storage (most components use this)
        /// </summary>
        [Obsolete("Reserved for future use.", true)]
        Singleton = 2 * 0x0000_1000_0000_0000u,
    }
    
    internal enum Wildcard : ulong
    {
        /// <summary>
        /// Plain Components (no key)
        /// </summary>
        Plain = KeyCategory.Plain,
        
        /// <summary>
        /// Relation (Target is an entity)
        /// </summary>
        Relation = KeyCategory.Entity,

        /// <summary>
        /// Object Link  (target is an instance of its own component backing type)
        /// </summary>
        Link2 = KeyCategory.Link,

        /// <summary>
        /// Any Target (Non-Plain)
        /// </summary>
        Target2 = Relation | Link2,
        
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
    
    internal Match(ulong value) => Value = value & Key.Mask;

    private Match(Wildcard wildcard) => Value = (ulong) wildcard;

    
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
    /// <para>This is an intentional feature, and the user is protected by the fact that the default is <see cref="Key.Plain"/>.</para>
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
    public static readonly Match Any = new(Wildcard.Any);
    
    /// <summary>
    /// <para>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>.
    /// This matches only components with no secondary key. (it's generally recommended to match by Key default instead)
    /// </para>
    /// </summary>
    public static readonly Match Plain = default;
    
    
    public const ulong AnyValue = (ulong) Wildcard.Any;
    public const ulong TargetValue = (ulong) Wildcard.Target;
    public const ulong LinkValue = (ulong) Wildcard.Link;
    public const ulong EntityValue = (ulong) Wildcard.Entity;
    
    
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
    public static readonly Match Plain = Match.Plain;

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