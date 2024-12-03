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
    /// Category / Kind of the Key.
    /// </summary>
    public Kind Category => (Kind) (Value & CategoryMask);


    /// <summary>
    /// Create a Key for an Entity-Entity relationship.
    /// Used to set targets of Object Links. 
    /// </summary>
    public static Key Of(Entity entity) => new(entity); /* KeyCategory already set by Entity*/

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
        Kind.Entity => new Entity(this).ToString(),
        Kind.Link => $"Link {Value:x16}", //TODO: format nicely.

        _ => $"?-{Value:x16}"
    };
    
    /// <summary>
    /// Implicit cast to Match term for use in Match Expressions.
    /// </summary>
    public static implicit operator Match(Key self) => new(self);

    internal const ulong BaseFlag = 0x0000_E000_0000_0000u;
    internal const ulong HeaderMask = 0xFFFF_0000_0000_0000u;
    internal const ulong KeyMask = ~0xFFFF_0000_0000_0000u;
    internal const ulong CategoryMask = 0x0000_F000_0000_0000u;

}