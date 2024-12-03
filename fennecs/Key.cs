using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Secondary Key for a Component type expression - used in relations, object links, etc.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct Key
{
    [FieldOffset(0)] internal readonly ulong Value;
    
    [FieldOffset(0)] 
    internal readonly int Index;

    internal Key(ulong value)
    {
        Debug.Assert((value & HeaderMask) == 0, "Keys may not have header bits set.");
        Value = value;
    }

    internal Key(Entity entity) => Value = entity.Value & KeyMask;
    
    
    /// <summary>
    /// Implicit conversion from Entity to Key.
    /// </summary>
    public static implicit operator Key(Entity entity) => entity.Key;

    /// <summary>
    /// Implicit conversion from Key to Entity.
    /// </summary>
    /// <remarks>
    /// The entity must be alive.
    /// </remarks>
    public static implicit operator Entity(Key self) => new LiveEntity(self).Entity;
    
    
    /// <summary>
    /// Category / Kind of the Key.
    /// </summary>
    public Kind Category => (Kind) (Value & CategoryMask);


    /// <summary>
    /// Create a Key for an Entity-Entity relationship.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of(Entity entity) => entity.Key; /* KeyCategory already set by Entity*/

    /// <summary>
    /// Create a Key for a tracked object and the backing Object Link type.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of<L>(L link) where L : class => new((ulong) Kind.Link | LanguageType<L>.LinkId | (uint) link.GetHashCode());

    /// <summary>
    /// Create a Key for a tracked object and the backing Object Link type.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of(object link) => new((ulong) Kind.Link | LanguageType.LinkId(link) | (uint) link.GetHashCode());

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
        /// Specific Object Link
        /// </summary>
        Link = 0x0000_8000_0000_0000u,
        
        /// <summary>
        /// Specific Entity
        /// </summary>
        Entity = 0x0000_E000_0000_0000u,
    }


    /// <summary>
    /// Is this Key representing an Entity Relation?
    /// </summary>
    public bool IsEntity => Category == Kind.Entity;

    /// <summary>
    /// Is this Key representing an Object Link?
    /// </summary>
    public bool IsLink => Category == Kind.Link;

    /// <inheritdoc />
    public override string ToString() => Category switch
    {
        Kind.Plain => "Plain",
        Kind.Entity => new LiveEntity(this).ToString(),
        Kind.Link => $"Link {Value:x16}", //TODO: format nicely.

        _ => $"?-{Value:x16}"
    };
    
    /// <summary>
    /// Implicit cast to Match term for use in Match Expressions.
    /// </summary>
    public static implicit operator Match(Key self) => new(self);
}

/// <summary>
/// A LiveEntity is a special Entity that's guaranteed to be alive.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly ref struct LiveEntity
{
    [FieldOffset(0)] private readonly ulong Raw;

    [FieldOffset(0)] internal readonly int Index;

    [FieldOffset(4)] private readonly World.Id WorldId;

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
    internal LiveEntity(Entity id)
    {
        Debug.Assert(id.Alive, $"Cannot create LiveEntity from dead Entity {id}!");
        Raw = id.Value & Key.KeyMask;
    }

    internal LiveEntity(ulong value)
    {
        Raw = value;
    }

    /// <summary>
    /// Returns the actual Entity this LiveEntity refers to.
    /// </summary>
    /// <param name="self">a LiveEntity</param>
    /// <returns>the entity</returns>
    public static implicit operator Entity(LiveEntity self) => self.World[self];

    /// <summary>
    /// Returns the actual World this LiveEntity refers to.
    /// </summary>
    public World World => World.Get(WorldId);

    /// <summary>
    /// Returns the actual Entity this LiveEntity refers to.
    /// </summary>
    public Entity Entity => this;

    /// <inheritdoc />
    public override string ToString() => $"E-{WorldId}:{Index:x8} live";
}