using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Secondary Key for a Component type expression - used in relations, object links, etc.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Key
{
    internal ulong Value { get; }

    internal Key(ulong value)
    {
        Debug.Assert((value & HeaderMask) == 0, "Keys may not have header bits set.");
        Value = value;
    }

    internal Key(Identity identity) => Value = identity.Value & KeyMask;
    
    public Kind Category => (Kind) (Value & CategoryMask);


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
}

/// <summary>
/// A LiveEntity is a special Identity that's guaranteed to be alive.
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
    internal LiveEntity(Identity id)
    {
        Debug.Assert(id.Alive, $"Cannot create LiveEntity from dead-Entity {id}!");
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
    public static implicit operator Identity(LiveEntity self) => self.World[self];

    /// <summary>
    /// Returns the actual World this LiveEntity refers to.
    /// </summary>
    public World World => World.Get(WorldId);

    /// <inheritdoc />
    public override string ToString() => $"E-{WorldId}:{Index:x8} live";
}