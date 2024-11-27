using System.Diagnostics;

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

    public ulong Category => (Value & CategoryMask) >> 44;
    
    
    public static Key Of(Entity entity) => new(entity.Id.Value & KeyMask); /* KeyCategory already set by Entity*/
    public static Key Of<L>(L link) where L : class => new((ulong) KeyCategory.Link | LanguageType<L>.LinkId | (uint) link.GetHashCode());
    
    private const ulong HeaderMask = 0xFFFF_0000_0000_0000u;
    private const ulong KeyMask = ~0xFFFF_0000_0000_0000u;
    private const ulong CategoryMask = 0x0000_F000_0000_0000u;
    
    /// <summary>
    /// Category of the Key.
    /// </summary>
    public enum KeyCategory : ulong
    {
        /// <summary>
        /// Plain Component (no secondary key / relations)
        /// </summary>
        Plain = default,
        
        /// <summary>
        /// Object Link (target Object)
        /// </summary>
        Link = 0x0000_1000_0000_0000u,

        /// <summary>
        /// Relation (targets Entity)
        /// </summary>
        Entity = 0x0000_E000_0000_0000u,
    }
}